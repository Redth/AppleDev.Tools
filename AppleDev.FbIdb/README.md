# AppleDev.FbIdb

A C# client library for Facebook's [IDB (iOS Development Bridge)](https://github.com/facebook/idb) companion, providing a full .NET API for controlling iOS simulators and devices.

## Features

- **Complete gRPC API Coverage**: Wraps all IDB companion gRPC methods
- **Companion Process Management**: Automatically manages the idb_companion process lifecycle
- **Flexible Binary Location**: Supports bundled binary, Homebrew installation, or custom paths
- **Async/Await Support**: Fully asynchronous API with `CancellationToken` support
- **Streaming Support**: Handles streaming operations for logs, video, and test results
- **Strong Typing**: All API responses mapped to strongly-typed C# models

## Requirements

- **macOS** (required - IDB only works on macOS)
- **.NET 8.0** or later
- **Xcode** with iOS Simulator support
- **idb_companion** binary (installed via Homebrew or bundled)

## Installation

### Install idb_companion via Homebrew (Recommended)

```bash
brew tap facebook/fb
brew install idb-companion
```

### Install NuGet Package

```bash
dotnet add package AppleDev.FbIdb
```

## Quick Start

```csharp
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;

// Create a client for a specific simulator UDID
await using var client = new IdbClient("SIMULATOR-UDID-HERE");

// Connect to the companion
await client.ConnectAsync();

// Get target information
var description = await client.DescribeAsync();
Console.WriteLine($"Connected to: {description.Name} ({description.OsVersion})");

// List installed apps
var apps = await client.ListAppsAsync();
foreach (var app in apps)
{
    Console.WriteLine($"  {app.BundleId} - {app.Name}");
}

// Take a screenshot
var screenshot = await client.ScreenshotAsync();
await File.WriteAllBytesAsync("screenshot.png", screenshot.ImageData);

// Set simulated location
await client.SetLocationAsync(37.7749, -122.4194); // San Francisco
```

## API Overview

### Connection & Management
- `ConnectAsync()` - Connect to the companion
- `DescribeAsync()` - Get target description
- `LogAsync()` - Stream device/companion logs

### App Lifecycle
- `InstallAsync()` - Install apps, xctest bundles, dylibs, dsyms, frameworks
- `UninstallAsync()` - Uninstall an app
- `LaunchAsync()` - Launch an app with arguments and environment
- `TerminateAsync()` - Terminate a running app
- `ListAppsAsync()` - List installed apps

### Media & Screenshots
- `ScreenshotAsync()` - Take a screenshot (automatically falls back to simctl for simulators)
- `AddMediaAsync()` - Add photos/videos to the device

### Interaction (HID)
- `TapAsync()` - Tap at a point
- `SwipeAsync()` - Perform swipe gesture
- `PressButtonAsync()` - Press hardware buttons (Home, Lock, etc.)
- `SendKeyAsync()` - Send keyboard input
- `FocusAsync()` - Focus the simulator window

### Settings & Permissions
- `ApprovePermissionAsync()` - Grant app permissions
- `RevokePermissionAsync()` - Revoke app permissions
- `SetSettingAsync()` / `GetSettingAsync()` - Manage device settings
- `SetHardwareKeyboardAsync()` - Enable/disable hardware keyboard
- `ClearKeychainAsync()` - Clear the keychain
- `SetLocationAsync()` - Set simulated GPS location

### File Operations
- `ListFilesAsync()` - List directory contents
- `MakeDirAsync()` - Create directories
- `PushAsync()` / `PullAsync()` - Transfer files to/from device
- `MoveAsync()` / `RemoveAsync()` - Move/delete files

### Crash Logs
- `ListCrashLogsAsync()` - List crash logs
- `GetCrashLogAsync()` - Get crash log contents
- `DeleteCrashLogsAsync()` - Delete crash logs

### XCTest
- `ListTestBundlesAsync()` - List installed test bundles
- `ListTestsAsync()` - List tests in a bundle
- `RunTestsAsync()` - Run tests and get results
- `RunTestsStreamAsync()` - Stream test results as they complete

### Notifications & Misc
- `SendNotificationAsync()` - Send push notifications
- `OpenUrlAsync()` - Open a URL
- `SimulateMemoryWarningAsync()` - Trigger memory warning

## Configuration

```csharp
var options = new IdbCompanionOptions
{
    // Use a custom companion binary path
    CompanionPath = "/path/to/idb_companion",
    
    // Specify a fixed gRPC port (0 = auto-assign)
    GrpcPort = 0,
    
    // Adjust timeouts
    StartupTimeout = TimeSpan.FromSeconds(30),
    OperationTimeout = TimeSpan.FromSeconds(60),
    
    // Enable verbose logging
    VerboseLogging = true
};

await using var client = new IdbClient("UDID", options);
```

### Environment Variable

You can also set the companion path via environment variable:

```bash
export IDB_COMPANION_PATH=/path/to/idb_companion
```

## Connecting to Existing Companion

If you have an idb_companion already running:

```csharp
// Connect to an existing gRPC server
var client = IdbClient.ConnectToExisting("http://localhost:12345");
```

## Running XCTests

```csharp
var request = new XctestRunRequest
{
    TestBundleId = "com.example.MyAppTests",
    Mode = new XctestMode.Application("com.example.MyApp"),
    CollectCoverage = true,
    ReportActivities = true
};

var result = await client.RunTestsAsync(request);

Console.WriteLine($"Status: {result.Status}");
foreach (var test in result.Results)
{
    var status = test.Status == TestStatus.Passed ? "✓" : "✗";
    Console.WriteLine($"  {status} {test.FullName} ({test.Duration:F2}s)");
}
```

## Streaming Test Results

```csharp
await foreach (var test in client.RunTestsStreamAsync(request))
{
    Console.WriteLine($"[{test.Status}] {test.FullName}");
}
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Facebook IDB](https://github.com/facebook/idb) - The underlying iOS Development Bridge
- [gRPC](https://grpc.io/) - High-performance RPC framework
