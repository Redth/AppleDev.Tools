using AppleDev.FbIdb.Models;

namespace AppleDev.FbIdb;

/// <summary>
/// Interface for the IDB client that communicates with iOS simulators and devices.
/// </summary>
public interface IIdbClient : IAsyncDisposable, IDisposable
{
	/// <summary>
	/// The target UDID this client is connected to.
	/// </summary>
	string? TargetUdid { get; }

	/// <summary>
	/// Whether the client is connected.
	/// </summary>
	bool IsConnected { get; }

	// Connection & Management

	/// <summary>
	/// Connects to the target.
	/// </summary>
	Task<CompanionInfo> ConnectAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the target description.
	/// </summary>
	Task<TargetDescription> DescribeAsync(bool fetchDiagnostics = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Streams logs from the target or companion.
	/// </summary>
	IAsyncEnumerable<byte[]> LogAsync(LogSource source = LogSource.Target, IEnumerable<string>? arguments = null, CancellationToken cancellationToken = default);

	// App Lifecycle

	/// <summary>
	/// Installs an app, xctest bundle, dylib, dsym, or framework.
	/// </summary>
	Task<InstallResult> InstallAsync(string path, InstallDestination destination = InstallDestination.App, CancellationToken cancellationToken = default);

	/// <summary>
	/// Installs an app from a stream.
	/// </summary>
	Task<InstallResult> InstallAsync(Stream stream, InstallDestination destination = InstallDestination.App, string? nameHint = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Uninstalls an app.
	/// </summary>
	Task UninstallAsync(string bundleId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Launches an app.
	/// </summary>
	Task<LaunchResult> LaunchAsync(string bundleId, LaunchOptions? options = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Terminates a running app.
	/// </summary>
	Task TerminateAsync(string bundleId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists installed apps.
	/// </summary>
	Task<IReadOnlyList<InstalledApp>> ListAppsAsync(bool suppressProcessState = false, CancellationToken cancellationToken = default);

	// Media & Screenshots

	/// <summary>
	/// Takes a screenshot.
	/// </summary>
	Task<Screenshot> ScreenshotAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds media files to the device.
	/// </summary>
	Task AddMediaAsync(string filePath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds media from a stream.
	/// </summary>
	Task AddMediaAsync(Stream stream, CancellationToken cancellationToken = default);

	// Interaction (HID)

	/// <summary>
	/// Taps at a point on the screen.
	/// </summary>
	Task TapAsync(double x, double y, CancellationToken cancellationToken = default);

	/// <summary>
	/// Taps at a point on the screen.
	/// </summary>
	Task TapAsync(Point point, CancellationToken cancellationToken = default);

	/// <summary>
	/// Performs a swipe gesture.
	/// </summary>
	Task SwipeAsync(Point start, Point end, double duration = 0.5, CancellationToken cancellationToken = default);

	/// <summary>
	/// Presses a hardware button.
	/// </summary>
	Task PressButtonAsync(HardwareButton button, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a key press.
	/// </summary>
	Task SendKeyAsync(ulong keycode, CancellationToken cancellationToken = default);

	/// <summary>
	/// Focuses the simulator window.
	/// </summary>
	Task FocusAsync(CancellationToken cancellationToken = default);

	// Settings & Permissions

	/// <summary>
	/// Approves permissions for an app.
	/// </summary>
	Task ApprovePermissionAsync(string bundleId, IEnumerable<Permission> permissions, CancellationToken cancellationToken = default);

	/// <summary>
	/// Revokes permissions for an app.
	/// </summary>
	Task RevokePermissionAsync(string bundleId, IEnumerable<Permission> permissions, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets a device setting.
	/// </summary>
	Task SetSettingAsync(string name, string value, string? domain = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a device setting.
	/// </summary>
	Task<string> GetSettingAsync(string name, string? domain = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists available settings.
	/// </summary>
	Task<IReadOnlyList<string>> ListSettingsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Enables or disables the hardware keyboard.
	/// </summary>
	Task SetHardwareKeyboardAsync(bool enabled, CancellationToken cancellationToken = default);

	/// <summary>
	/// Clears the keychain.
	/// </summary>
	Task ClearKeychainAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the simulated location.
	/// </summary>
	Task SetLocationAsync(double latitude, double longitude, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the simulated location.
	/// </summary>
	Task SetLocationAsync(GeoLocation location, CancellationToken cancellationToken = default);

	// Contacts & Data

	/// <summary>
	/// Updates contacts from a file.
	/// </summary>
	Task UpdateContactsAsync(string filePath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Clears all contacts.
	/// </summary>
	Task ClearContactsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Clears the photos library.
	/// </summary>
	Task ClearPhotosAsync(CancellationToken cancellationToken = default);

	// Accessibility

	/// <summary>
	/// Gets accessibility information.
	/// </summary>
	Task<AccessibilityInfo> GetAccessibilityInfoAsync(Point? point = null, AccessibilityFormat format = AccessibilityFormat.Nested, CancellationToken cancellationToken = default);

	// File Operations

	/// <summary>
	/// Lists files in a directory.
	/// </summary>
	Task<IReadOnlyList<IdbFileInfo>> ListFilesAsync(string path, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a directory.
	/// </summary>
	Task MakeDirAsync(string path, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Moves or renames files.
	/// </summary>
	Task MoveAsync(IEnumerable<string> sourcePaths, string destinationPath, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes files or directories.
	/// </summary>
	Task RemoveAsync(IEnumerable<string> paths, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Pushes a file to the device.
	/// </summary>
	Task PushAsync(string localPath, string remotePath, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Pushes data from a stream to the device.
	/// </summary>
	Task PushAsync(Stream stream, string remotePath, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Pulls a file from the device.
	/// </summary>
	Task PullAsync(string remotePath, string localPath, FileContainer? container = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Pulls a file from the device to a stream.
	/// </summary>
	Task<byte[]> PullAsync(string remotePath, FileContainer? container = null, CancellationToken cancellationToken = default);

	// Crash Logs

	/// <summary>
	/// Lists crash logs.
	/// </summary>
	Task<IReadOnlyList<CrashLogInfo>> ListCrashLogsAsync(string? bundleId = null, DateTime? since = null, DateTime? before = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a crash log by name.
	/// </summary>
	Task<CrashLog> GetCrashLogAsync(string name, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes crash logs.
	/// </summary>
	Task<IReadOnlyList<CrashLogInfo>> DeleteCrashLogsAsync(string? bundleId = null, DateTime? since = null, DateTime? before = null, string? name = null, CancellationToken cancellationToken = default);

	// XCTest

	/// <summary>
	/// Lists installed test bundles.
	/// </summary>
	Task<IReadOnlyList<TestBundle>> ListTestBundlesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists tests in a bundle.
	/// </summary>
	Task<IReadOnlyList<string>> ListTestsAsync(string bundleName, string? appPath = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Runs tests and returns results.
	/// </summary>
	Task<XctestRunResult> RunTestsAsync(XctestRunRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Runs tests and streams results.
	/// </summary>
	IAsyncEnumerable<TestRunInfo> RunTestsStreamAsync(XctestRunRequest request, CancellationToken cancellationToken = default);

	// Notifications & Misc

	/// <summary>
	/// Sends a push notification.
	/// </summary>
	Task SendNotificationAsync(string bundleId, string jsonPayload, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens a URL.
	/// </summary>
	Task OpenUrlAsync(string url, CancellationToken cancellationToken = default);

	/// <summary>
	/// Simulates a memory warning.
	/// </summary>
	Task SimulateMemoryWarningAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// XCTest run request configuration.
/// </summary>
public class XctestRunRequest
{
	/// <summary>
	/// The test bundle ID to run.
	/// </summary>
	public string TestBundleId { get; set; } = string.Empty;

	/// <summary>
	/// The test mode.
	/// </summary>
	public XctestMode Mode { get; set; } = new XctestMode.Logic();

	/// <summary>
	/// Specific tests to run (empty = all tests).
	/// </summary>
	public IList<string> TestsToRun { get; set; } = new List<string>();

	/// <summary>
	/// Tests to skip.
	/// </summary>
	public IList<string> TestsToSkip { get; set; } = new List<string>();

	/// <summary>
	/// Arguments to pass to the test.
	/// </summary>
	public IList<string> Arguments { get; set; } = new List<string>();

	/// <summary>
	/// Environment variables.
	/// </summary>
	public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

	/// <summary>
	/// Test timeout in seconds.
	/// </summary>
	public ulong Timeout { get; set; }

	/// <summary>
	/// Report test activities.
	/// </summary>
	public bool ReportActivities { get; set; }

	/// <summary>
	/// Collect code coverage.
	/// </summary>
	public bool CollectCoverage { get; set; }

	/// <summary>
	/// Report attachments.
	/// </summary>
	public bool ReportAttachments { get; set; }

	/// <summary>
	/// Collect logs.
	/// </summary>
	public bool CollectLogs { get; set; }

	/// <summary>
	/// Wait for debugger to attach.
	/// </summary>
	public bool WaitForDebugger { get; set; }
}

/// <summary>
/// XCTest mode.
/// </summary>
public abstract record XctestMode
{
	/// <summary>
	/// Logic test mode (unit tests without app).
	/// </summary>
	public sealed record Logic : XctestMode;

	/// <summary>
	/// Application test mode.
	/// </summary>
	/// <param name="AppBundleId">The app bundle ID to test.</param>
	public sealed record Application(string AppBundleId) : XctestMode;

	/// <summary>
	/// UI test mode.
	/// </summary>
	/// <param name="AppBundleId">The app bundle ID to test.</param>
	/// <param name="TestHostAppBundleId">The test host app bundle ID.</param>
	public sealed record UI(string AppBundleId, string TestHostAppBundleId) : XctestMode;
}
