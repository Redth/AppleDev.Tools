# AGENTS.md

This file provides guidance for agentic coding assistants working with this repository.

## Build and Test Commands

### Build
```bash
# Build entire solution
dotnet build AppleDev.sln

# Build specific projects
dotnet build AppleDev/AppleDev.csproj
dotnet build AppleDev.Tool/AppleDev.Tool.csproj
```

### Test
```bash
# Run all tests (requires macOS with Xcode)
dotnet test AppleDev.Test/AppleDev.Test.csproj

# Run with detailed logging
dotnet test AppleDev.Test/AppleDev.Test.csproj --logger "console;verbosity=detailed"

# Run a single test
dotnet test AppleDev.Test/AppleDev.Test.csproj --filter "FullyQualifiedName~SimCtlTests.GetAnySimulators"
dotnet test AppleDev.Test/AppleDev.Test.csproj --filter "DisplayName=GetAnySimulators"

# Run all tests in a class
dotnet test AppleDev.Test/AppleDev.Test.csproj --filter "FullyQualifiedName~SimCtlTests"
```

### Pack
```bash
dotnet pack --output ./artifacts --configuration Release AppleDev/AppleDev.csproj
dotnet pack --output ./artifacts --configuration Release AppStoreConnectClient/AppStoreConnectClient.csproj
dotnet pack --output ./artifacts --configuration Release AppleDev.Tool/AppleDev.Tool.csproj
```

## Code Style

### Formatting
- **Indentation**: Tabs (4 spaces width)
- **Brace style**: Allman style - opening brace on new line
- **Naming**: PascalCase for classes, methods, properties; camelCase for parameters/locals
- **Nullable**: Nullable reference types enabled (`string?`, `T?`)
- **Implicit usings**: Enabled in all projects

### Imports
- Standard using statements at top of file
- Implicit usings enabled - avoid redundant imports
- Group by namespace, no blank lines between groups

### Type Guidelines
- Use C# 10+ features (pattern matching, switch expressions, file-scoped namespaces)
- Prefer `string?` over `string` for nullable types
- Use `Array.Empty<T>()` instead of `new T[0]`
- Prefer pattern matching: `if (item is not null)` instead of `if (item != null)`
- Use `ConfigureAwait(false)` in library code

### Async/Await
- All async methods accept `CancellationToken cancellationToken = default`
- Use `.ConfigureAwait(false)` in library code
- Return `Task<T>` or `Task`, avoid `async void`
- Prefix async methods with `Async` suffix

### Error Handling
- Throw `PlatformNotSupportedException` for macOS-only features
- Check `OperatingSystem.IsMacOS()` before calling macOS APIs
- Wrap external process calls in try-catch, return `ProcessResult` objects
- Use custom exceptions like `AppleApiException` for API errors

### Library Code (AppleDev, AppStoreConnectClient)
- Base class `XCRun` for tool wrappers (`SimCtl`, `XCDevice`, `Keychain`)
- Use `CliWrap` for process execution
- Inject `ILogger<T>` via constructor, use `NullLogger<T>.Instance` as default
- Return `ProcessResult(bool Success, string StdOut, string StdErr)` from CLI wrappers

### CLI Commands (AppleDev.Tool)
- Inherit from `Command<TSettings>` and implement `ExecuteAsync`
- Settings inherit from `FormattableOutputCommandSettings` for output formatting
- Use `OutputHelper.Output()` for formatted output (table, JSON, verbose)
- Access cancellation token via `context.GetData().CancellationToken`
- Use `Spectre.Console` for rich terminal output

### Testing
- Use xUnit with `[Fact]` attributes
- Inject `ITestOutputHelper` for test logging
- Use `XUnitLogger<T>` adapter for `ILogger<T>` in tests
- Use `[Collection("SimCtl")]` to group tests requiring shared simctl access
- Place test data in `testdata/` directory, access via `TestsBase.TestDataDirectory`
- Use `Assert.NotNull`, `Assert.NotEmpty`, `Record.Exception` for assertions

### CLI Tool Testing
```bash
dotnet pack AppleDev.Tool/AppleDev.Tool.csproj -o ./artifacts
dotnet tool uninstall -g AppleDev.Tool
dotnet tool install -g AppleDev.Tool --add-source ./artifacts
apple --help
```

## Architecture

- **AppleDev** - Core library wrapping Apple CLI tools (SimCtl, XCDevice, Xcode, Keychain, etc.)
- **AppleDev.Tool** - Spectre.Console.Cli-based CLI tool
- **AppStoreConnectClient** - REST API client with JWT auth
- **AppleDev.Test** - xUnit tests with macOS requirements

## Versioning

Uses Nerdbank.GitVersioning with `version.json` configuration.
