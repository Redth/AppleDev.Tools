# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

AppleDev.Tools is a .NET solution providing Apple development tooling through both a library API and a CLI tool. The solution consists of:
- **AppleDev** - Core library with Apple tool wrappers (SimCtl, XCDevice, Xcode, Keychain, etc.)
- **AppleDev.Tool** - Global .NET CLI tool (command name: `apple`)
- **AppStoreConnectClient** - App Store Connect API client library
- **AppleDev.Test** - xUnit test project

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
```

### Pack NuGet Packages
```bash
# Pack all projects
dotnet pack --output ./artifacts --configuration Release AppleDev/AppleDev.csproj
dotnet pack --output ./artifacts --configuration Release AppStoreConnectClient/AppStoreConnectClient.csproj
dotnet pack --output ./artifacts --configuration Release AppleDev.Tool/AppleDev.Tool.csproj
```

### Local CLI Tool Testing
```bash
# Install local tool build
dotnet pack AppleDev.Tool/AppleDev.Tool.csproj -o ./artifacts
dotnet tool uninstall -g AppleDev.Tool
dotnet tool install -g AppleDev.Tool --add-source ./artifacts

# Test the tool
apple --help
apple simulator list
```

## Architecture

### Core Library (AppleDev)

The library wraps native Apple command-line tools using **CliWrap** for process execution:

- **XCRun** - Base class that locates and invokes tools via `xcrun`
- **Xcode** - Locates Xcode installations using `xcode-select` and parses version info from plists
- **SimCtl** - Wraps `xcrun simctl` for iOS/watchOS/tvOS/visionOS simulator management
- **XCDevice** - Wraps `xcdevice` for listing and monitoring physical devices
- **Keychain** - Wraps `security` command for keychain operations
- **ProvisioningProfiles** - Parses .mobileprovision files using plist-cil
- **ALTool** - Wraps `altool` for App Store Connect uploads/validation
- **AppBundleReader** - Extracts metadata from .app bundles
- **CertificateRequestGenerator** - Generates certificate signing requests (CSRs)

All tools inherit from XCRun or use the base XCRun pattern for consistency. Logging is done through `Microsoft.Extensions.Logging.ILogger<T>`.

### CLI Tool (AppleDev.Tool)

Built with **Spectre.Console.Cli** for command parsing and rich terminal output. The CLI is structured as:

- **Program.cs** - Defines all commands with `CommandApp` and registers branches (simulator, device, keychain, provisioning, bundleids, certificate, ci)
- **Commands/** - Command implementations organized by feature:
  - `Simulators/` - Simulator management commands
  - `Devices/` - Device listing commands
  - `Keychain/` - Keychain operations
  - `AppStoreConnect/` - Provisioning profile and bundle ID commands
  - `CI/` - CI/CD automation commands
- **OutputHelper.cs** - Handles formatted output (Table, JSON, Verbose) using Spectre.Console
- **Extensions.cs** - Helper extension methods

All commands inherit from `Command<TSettings>` and implement `ExecuteAsync`. Commands use `FormattableOutputCommandSettings` for consistent output formatting.

### App Store Connect Client (AppStoreConnectClient)

REST API client for App Store Connect:

- **Client.cs** - Base HTTP client with JWT bearer authentication
- **Client.*.cs** - Partial classes for different API endpoints (BundleIds, Certificates, Profiles)
- **AppStoreConnectConfiguration** - Handles JWT generation from private key using ES256 algorithm
- **Models/** - Strongly-typed models for API responses (Profile, Certificate, BundleId, Device)
- **BaseModels/** - Generic response wrappers (ListResponse, ItemResponse, Request, etc.)

JWT tokens are auto-refreshed before expiration (19-minute lifetime).

## Platform Requirements

Most functionality requires **macOS** with **Xcode** installed. The library checks `OperatingSystem.IsMacOS()` and throws `PlatformNotSupportedException` for macOS-only features.

Tests run on **macOS-15** in CI (see `.github/workflows/build.yml`).

## Versioning

Uses **Nerdbank.GitVersioning** with configuration in `version.json`. Version is based on tags and git history.

## App Store Connect Authentication

Commands requiring App Store Connect API access need these environment variables:
- `APP_STORE_CONNECT_KEY_ID` - API Key ID
- `APP_STORE_CONNECT_ISSUER_ID` - Issuer ID (UUID)
- `APP_STORE_CONNECT_PRIVATE_KEY` - Private key (.p8 file content as base64)

The `AppStoreConnectConfiguration` class handles JWT token generation and refresh.

## CI/CD Patterns

The `ci` command branch provides automation for CI environments:
- `ci provision` - Sets up keychain, imports certificates, downloads provisioning profiles
- `ci deprovision` - Cleans up temporary keychains
- `ci secret` - Converts certificates/keys to base64 for CI secrets
- `ci base64-to-file` - Decodes base64 environment variables to files
- `ci env-to-file` - Writes environment variables directly to files

These commands are designed for GitHub Actions, Azure Pipelines, and other CI systems.

## Code Style

- Uses C# 10+ features with nullable reference types enabled
- Async/await pattern with `CancellationToken` support throughout
- JSON parsing uses both Newtonsoft.Json (SimCtl) and System.Text.Json (AppStoreConnectClient)
- Extensive use of pattern matching and modern C# syntax
- ILogger integration for diagnostic logging
