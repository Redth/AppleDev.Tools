# Copilot Instructions for AppleDev.Tools

This repository contains .NET libraries and tools for Apple development workflows, wrapping Apple CLI tools like simctl, xcrun, xcodebuild, and providing an App Store Connect API client.

## Project Structure

- **AppleDev** - Core library providing wrappers for Apple CLI tools (simctl, xcdevice, xcodebuild, security)
- **AppleDev.Tool** - Global .NET CLI tool (`apple` command) built with Spectre.Console
- **AppStoreConnectClient** - REST API client for App Store Connect with JWT authentication
- **AppleDev.Mcp.Server** - Model Context Protocol server for Apple development tools
- **AppleDev.Test** - xUnit test suite (requires macOS with Xcode)

## Technology Stack

- **.NET 8.0** with C# 10+ features
- **CliWrap** for process execution
- **Spectre.Console.Cli** for rich CLI experiences
- **xUnit** for testing
- **Nerdbank.GitVersioning** for semantic versioning

## Platform Requirements

- Most functionality requires **macOS** with Xcode installed
- Code should check `OperatingSystem.IsMacOS()` and throw `PlatformNotSupportedException` when necessary
- Tests are designed to run on macOS only

## Development Workflow

### Build & Test
```bash
# Build the entire solution
dotnet build AppleDev.sln

# Run tests (macOS only)
dotnet test AppleDev.Test/AppleDev.Test.csproj

# Pack for distribution
dotnet pack --output ./artifacts --configuration Release
```

### Testing the CLI Tool
```bash
dotnet pack AppleDev.Tool/AppleDev.Tool.csproj -o ./artifacts
dotnet tool uninstall -g AppleDev.Tool
dotnet tool install -g AppleDev.Tool --add-source ./artifacts
apple --help
```

## Code Conventions

### General C# Style
- Use **tabs** for indentation (4 spaces width)
- **Allman brace style** - opening braces on new line
- **PascalCase** for types, methods, properties; **camelCase** for parameters/locals
- Enable **nullable reference types** (`string?`, `T?`)
- Use **file-scoped namespaces** (C# 10+)
- Leverage **pattern matching** and **switch expressions**

### Async Patterns
- All async methods accept `CancellationToken cancellationToken = default`
- Use `.ConfigureAwait(false)` in library code
- Suffix async methods with `Async`
- Return `Task<T>` or `Task`, never `async void`

### Library Design (AppleDev, AppStoreConnectClient)
- Base class `XCRun` for CLI tool wrappers
- Use `CliWrap` for all external process execution
- Inject `ILogger<T>` via constructor with `NullLogger<T>.Instance` as default
- Return `ProcessResult(bool Success, string StdOut, string StdErr)` from process wrappers
- Throw `PlatformNotSupportedException` for macOS-only features

### CLI Tool Design (AppleDev.Tool)
- Commands inherit from `Command<TSettings>` and implement `ExecuteAsync`
- Settings classes inherit from `FormattableOutputCommandSettings` for consistent output
- Use `OutputHelper.Output()` for table, JSON, and verbose formatting
- Access cancellation token via `context.GetData().CancellationToken`
- Use `Spectre.Console` for rich terminal UI

### Testing Patterns
- Use xUnit `[Fact]` attributes
- Inject `ITestOutputHelper` for logging
- Use `XUnitLogger<T>` adapter for `ILogger<T>` in tests
- Group tests with `[Collection("SimCtl")]` for shared resource access
- Test data files go in `testdata/` directory
- Use `Assert.NotNull`, `Assert.NotEmpty`, `Record.Exception`

## Dependencies

- Prefer using existing dependencies over adding new ones
- No need to add xUnit, Spectre.Console, or CliWrap - already included
- External dependencies should target .NET 8.0

## Error Handling

- Wrap external process calls in try-catch
- Use custom exceptions like `AppleApiException` for API errors
- Always check platform compatibility with `OperatingSystem.IsMacOS()`
- Log errors using injected `ILogger<T>`

## Documentation

- Keep README.md updated with new features
- Use XML doc comments for public APIs
- CLI commands should have descriptive help text and examples

## Best Practices

- **Minimal changes**: Make surgical, targeted modifications
- **Test on macOS**: Most functionality requires Xcode
- **Use existing patterns**: Follow conventions in similar code
- **Leverage tooling**: Use CliWrap, not raw Process APIs
- **Format output**: Support table, JSON, and verbose modes in CLI commands
- **Cancellation support**: Always accept and respect CancellationToken

## What NOT to Do

- Don't use `Process.Start` directly - use CliWrap
- Don't ignore platform checks - code should gracefully handle non-macOS
- Don't add async void methods
- Don't use spaces for indentation
- Don't use K&R brace style - use Allman (braces on new line)
- Don't bypass ConfigureAwait in library code
