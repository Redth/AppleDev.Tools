namespace AppleDev.FbIdb;

/// <summary>
/// Configuration options for the IDB companion process.
/// </summary>
public class IdbCompanionOptions
{
	/// <summary>
	/// Optional custom path to the idb_companion binary.
	/// If not specified, the bundled binary will be used.
	/// </summary>
	public string? CompanionPath { get; set; }

	/// <summary>
	/// The port number for the gRPC server. Default is 0 (auto-assign).
	/// </summary>
	public int GrpcPort { get; set; } = 0;

	/// <summary>
	/// Timeout for companion startup. Default is 30 seconds.
	/// </summary>
	public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Timeout for companion shutdown. Default is 10 seconds.
	/// </summary>
	public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Timeout for gRPC operations. Default is 60 seconds.
	/// </summary>
	public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(60);

	/// <summary>
	/// Enable verbose logging from the companion process.
	/// </summary>
	public bool VerboseLogging { get; set; } = false;

	/// <summary>
	/// The UDID of the target simulator or device to connect to.
	/// </summary>
	public string? TargetUdid { get; set; }

	/// <summary>
	/// Environment variable name for custom companion path override.
	/// </summary>
	public const string CompanionPathEnvironmentVariable = "IDB_COMPANION_PATH";
}
