namespace AppleDev.FbIdb.Models;

/// <summary>
/// Information about a crash log.
/// </summary>
public class CrashLogInfo
{
	/// <summary>
	/// The crash log file name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The bundle identifier of the crashed app.
	/// </summary>
	public string BundleId { get; set; } = string.Empty;

	/// <summary>
	/// The process name.
	/// </summary>
	public string ProcessName { get; set; } = string.Empty;

	/// <summary>
	/// The parent process name.
	/// </summary>
	public string ParentProcessName { get; set; } = string.Empty;

	/// <summary>
	/// The process identifier.
	/// </summary>
	public ulong ProcessIdentifier { get; set; }

	/// <summary>
	/// The parent process identifier.
	/// </summary>
	public ulong ParentProcessIdentifier { get; set; }

	/// <summary>
	/// The timestamp of the crash.
	/// </summary>
	public ulong Timestamp { get; set; }

	/// <summary>
	/// The crash timestamp as a DateTime.
	/// </summary>
	public DateTime? CrashTime => Timestamp > 0
		? DateTimeOffset.FromUnixTimeSeconds((long)Timestamp).LocalDateTime
		: null;
}

/// <summary>
/// Full crash log with contents.
/// </summary>
public class CrashLog : CrashLogInfo
{
	/// <summary>
	/// The full crash log contents.
	/// </summary>
	public string Contents { get; set; } = string.Empty;
}
