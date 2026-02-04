namespace AppleDev.FbIdb.Models;

/// <summary>
/// Information about an installed application.
/// </summary>
public class InstalledApp
{
	/// <summary>
	/// The bundle identifier.
	/// </summary>
	public string BundleId { get; set; } = string.Empty;

	/// <summary>
	/// The application name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Supported architectures.
	/// </summary>
	public IReadOnlyList<string> Architectures { get; set; } = Array.Empty<string>();

	/// <summary>
	/// The install type (e.g., "user", "system").
	/// </summary>
	public string InstallType { get; set; } = string.Empty;

	/// <summary>
	/// The process state.
	/// </summary>
	public AppProcessState ProcessState { get; set; }

	/// <summary>
	/// Whether the app is debuggable.
	/// </summary>
	public bool Debuggable { get; set; }

	/// <summary>
	/// The process identifier if running.
	/// </summary>
	public ulong ProcessIdentifier { get; set; }
}

/// <summary>
/// Application process state.
/// </summary>
public enum AppProcessState
{
	/// <summary>
	/// State is unknown.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// App is not running.
	/// </summary>
	NotRunning = 1,

	/// <summary>
	/// App is running.
	/// </summary>
	Running = 2
}
