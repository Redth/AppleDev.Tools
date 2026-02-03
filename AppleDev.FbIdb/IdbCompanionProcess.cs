using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppleDev.FbIdb;

/// <summary>
/// Manages the lifecycle of an idb_companion process.
/// </summary>
public sealed partial class IdbCompanionProcess : IAsyncDisposable, IDisposable
{
	private readonly ILogger _logger;
	private readonly IdbCompanionOptions _options;
	private readonly IdbCompanionLocator _locator;
	private Process? _process;
	private readonly StringBuilder _stdOut = new();
	private readonly StringBuilder _stdErr = new();
	private bool _disposed;
	private TaskCompletionSource<int>? _grpcPortTcs;

	/// <summary>
	/// The gRPC port the companion is listening on.
	/// </summary>
	public int? GrpcPort { get; private set; }

	/// <summary>
	/// The target UDID this companion is connected to.
	/// </summary>
	public string? TargetUdid { get; private set; }

	/// <summary>
	/// Whether the companion process is currently running.
	/// </summary>
	public bool IsRunning => _process is { HasExited: false };

	/// <summary>
	/// The gRPC address to connect to.
	/// </summary>
	public string? GrpcAddress => GrpcPort.HasValue ? $"http://localhost:{GrpcPort}" : null;

	/// <summary>
	/// Creates a new IdbCompanionProcess instance.
	/// </summary>
	/// <param name="options">Configuration options.</param>
	/// <param name="logger">Optional logger.</param>
	public IdbCompanionProcess(IdbCompanionOptions? options = null, ILogger<IdbCompanionProcess>? logger = null)
	{
		_options = options ?? new IdbCompanionOptions();
		_logger = logger ?? NullLogger<IdbCompanionProcess>.Instance;
		_locator = new IdbCompanionLocator(_options, null);
		// Initialize TargetUdid from options if set
		TargetUdid = _options.TargetUdid;
	}

	/// <summary>
	/// Starts the idb_companion process for the specified target.
	/// </summary>
	/// <param name="targetUdid">The UDID of the simulator or device to connect to.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The gRPC port the companion is listening on.</returns>
	public async Task<int> StartAsync(string targetUdid, CancellationToken cancellationToken = default)
	{
		ThrowIfNotMacOS();
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (IsRunning)
		{
			throw new InvalidOperationException("Companion process is already running");
		}

		TargetUdid = targetUdid;
		var companionPath = _locator.Locate();

		_logger.LogInformation("Starting idb_companion for target {TargetUdid}", targetUdid);
		_logger.LogDebug("Using companion binary: {Path}", companionPath);

		var args = new List<string>
		{
			"--udid", targetUdid,
			"--grpc-port", _options.GrpcPort.ToString()
		};

		if (_options.VerboseLogging)
		{
			args.Add("--log");
			args.Add("DEBUG");
		}

		var startInfo = new ProcessStartInfo
		{
			FileName = companionPath,
			Arguments = string.Join(" ", args),
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		_grpcPortTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		_stdOut.Clear();
		_stdErr.Clear();

		_process = new Process { StartInfo = startInfo };
		_process.OutputDataReceived += OnOutputDataReceived;
		_process.ErrorDataReceived += OnErrorDataReceived;

		if (!_process.Start())
		{
			throw new InvalidOperationException("Failed to start idb_companion process");
		}

		_process.BeginOutputReadLine();
		_process.BeginErrorReadLine();

		_logger.LogDebug("idb_companion process started with PID {Pid}", _process.Id);

		// Wait for the gRPC port to be reported or timeout
		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutCts.CancelAfter(_options.StartupTimeout);

		try
		{
			var port = await _grpcPortTcs.Task.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
			GrpcPort = port;
			_logger.LogInformation("idb_companion ready on port {Port}", port);
			return port;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			// Timeout occurred
			await StopAsync().ConfigureAwait(false);
			throw new TimeoutException($"idb_companion did not report gRPC port within {_options.StartupTimeout.TotalSeconds} seconds. StdErr: {_stdErr}");
		}
	}

	/// <summary>
	/// Stops the companion process.
	/// </summary>
	public async Task StopAsync()
	{
		if (_process is null || _process.HasExited)
		{
			_logger.LogDebug("Companion process is not running");
			return;
		}

		_logger.LogInformation("Stopping idb_companion process (PID {Pid})", _process.Id);

		try
		{
			// Try graceful shutdown first
			_process.Kill(entireProcessTree: true);

			using var cts = new CancellationTokenSource(_options.ShutdownTimeout);
			await _process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error while stopping companion process");
		}
		finally
		{
			GrpcPort = null;
			TargetUdid = null;
		}
	}

	private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		if (e.Data is null) return;

		_stdOut.AppendLine(e.Data);
		_logger.LogDebug("idb_companion stdout: {Output}", e.Data);

		// Parse the gRPC port from output
		// Expected format: "{"grpc_port": 12345}"
		TryParseGrpcPort(e.Data);
	}

	private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		if (e.Data is null) return;

		_stdErr.AppendLine(e.Data);
		_logger.LogDebug("idb_companion stderr: {Output}", e.Data);

		// The port info might also be in stderr
		TryParseGrpcPort(e.Data);
	}

	private void TryParseGrpcPort(string data)
	{
		if (_grpcPortTcs is null || _grpcPortTcs.Task.IsCompleted)
			return;

		// Try to parse JSON format: {"grpc_port": 12345}
		// This is the authoritative format from idb_companion
		var match = GrpcPortRegex().Match(data);
		if (match.Success && int.TryParse(match.Groups[1].Value, out var port) && port > 0)
		{
			_logger.LogDebug("Parsed gRPC port from JSON: {Port}", port);
			_grpcPortTcs.TrySetResult(port);
			return;
		}

		// Also try format: Swift server started on [IPv6]::/:::12345
		match = SwiftServerPortRegex().Match(data);
		if (match.Success && int.TryParse(match.Groups[1].Value, out port) && port > 0)
		{
			_logger.LogDebug("Parsed gRPC port from Swift server output: {Port}", port);
			_grpcPortTcs.TrySetResult(port);
		}
	}

	// Matches: {"grpc_port":12345} or {"grpc_port": 12345}
	[GeneratedRegex(@"""grpc_port""\s*:\s*(\d+)", RegexOptions.Compiled)]
	private static partial Regex GrpcPortRegex();

	// Matches: Swift server started on [IPv6]::/:::12345
	[GeneratedRegex(@"Swift server started on.*:(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
	private static partial Regex SwiftServerPortRegex();

	private static void ThrowIfNotMacOS()
	{
		if (!OperatingSystem.IsMacOS())
		{
			throw new PlatformNotSupportedException(
				"IDB (iOS Development Bridge) is only supported on macOS.");
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		if (_process is not null)
		{
			try
			{
				if (!_process.HasExited)
				{
					_process.Kill(entireProcessTree: true);
					_process.WaitForExit((int)_options.ShutdownTimeout.TotalMilliseconds);
				}
				
				_process.OutputDataReceived -= OnOutputDataReceived;
				_process.ErrorDataReceived -= OnErrorDataReceived;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Error disposing companion process");
			}
			finally
			{
				_process.Dispose();
				_process = null;
			}
		}

		GrpcPort = null;
		TargetUdid = null;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;

		await StopAsync().ConfigureAwait(false);
		Dispose();
	}
}
