using System.Net;
using System.Runtime.CompilerServices;
using AppleDev.FbIdb.Models;
using Grpc.Core;
using Grpc.Net.Client;
using Idb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using GrpcPoint = Idb.Point;
using GrpcFileContainer = Idb.FileContainer;
using GrpcSetting = Idb.Setting;
using GrpcCompanionInfo = Idb.CompanionInfo;
using GrpcTargetDescription = Idb.TargetDescription;
using GrpcCrashLogInfo = Idb.CrashLogInfo;
using CompanionInfo = AppleDev.FbIdb.Models.CompanionInfo;
using TargetDescription = AppleDev.FbIdb.Models.TargetDescription;
using CrashLogInfo = AppleDev.FbIdb.Models.CrashLogInfo;
using Point = AppleDev.FbIdb.Models.Point;
using FileContainer = AppleDev.FbIdb.Models.FileContainer;
using ScreenDimensions = AppleDev.FbIdb.Models.ScreenDimensions;

namespace AppleDev.FbIdb;

/// <summary>
/// Client for communicating with iOS simulators and devices via IDB companion.
/// </summary>
public sealed class IdbClient : IIdbClient
{
	private readonly ILogger _logger;
	private readonly IdbCompanionOptions _options;
	private readonly IdbCompanionProcess _companionProcess;
	private readonly bool _ownsCompanionProcess;
	private GrpcChannel? _channel;
	private CompanionService.CompanionServiceClient? _client;
	private bool _disposed;

	/// <summary>
	/// Static constructor to enable HTTP/2 over unencrypted connections (h2c).
	/// Required for gRPC over plain HTTP.
	/// </summary>
	static IdbClient()
	{
		// Enable h2c (HTTP/2 over plaintext) support
		AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
	}

	/// <inheritdoc />
	public string? TargetUdid => _companionProcess.TargetUdid;

	/// <inheritdoc />
	public bool IsConnected => _channel is not null && _companionProcess.IsRunning;

	/// <summary>
	/// Creates a new IdbClient that manages its own companion process.
	/// </summary>
	/// <param name="targetUdid">The UDID of the target simulator or device.</param>
	/// <param name="options">Configuration options.</param>
	/// <param name="logger">Optional logger.</param>
	public IdbClient(string targetUdid, IdbCompanionOptions? options = null, ILogger<IdbClient>? logger = null)
	{
		_logger = logger ?? NullLogger<IdbClient>.Instance;
		_options = options ?? new IdbCompanionOptions();
		_options.TargetUdid = targetUdid;
		
		// Create a logger for the companion process if we have one
		ILogger<IdbCompanionProcess>? processLogger = null;
		if (logger is not null)
		{
			// Use the same logger factory output
			processLogger = new LoggerAdapter<IdbCompanionProcess>(logger);
		}
		
		_companionProcess = new IdbCompanionProcess(_options, processLogger);
		_ownsCompanionProcess = true;
	}

	/// <summary>
	/// Creates a new IdbClient using an existing companion process.
	/// </summary>
	/// <param name="companionProcess">The running companion process.</param>
	/// <param name="options">Configuration options.</param>
	/// <param name="logger">Optional logger.</param>
	public IdbClient(IdbCompanionProcess companionProcess, IdbCompanionOptions? options = null, ILogger<IdbClient>? logger = null)
	{
		_logger = logger ?? NullLogger<IdbClient>.Instance;
		_options = options ?? new IdbCompanionOptions();
		_companionProcess = companionProcess ?? throw new ArgumentNullException(nameof(companionProcess));
		_ownsCompanionProcess = false;
	}

	/// <summary>
	/// Creates a new IdbClient connected to an existing gRPC address.
	/// </summary>
	/// <param name="grpcAddress">The gRPC address (e.g., "http://localhost:12345").</param>
	/// <param name="options">Configuration options.</param>
	/// <param name="logger">Optional logger.</param>
	public static IdbClient ConnectToExisting(string grpcAddress, IdbCompanionOptions? options = null, ILogger<IdbClient>? logger = null)
	{
		var client = new IdbClient(options ?? new IdbCompanionOptions(), logger ?? NullLogger<IdbClient>.Instance);
		client.ConnectToAddress(grpcAddress);
		return client;
	}

	private IdbClient(IdbCompanionOptions options, ILogger logger)
	{
		_logger = logger;
		_options = options;
		_companionProcess = new IdbCompanionProcess(options, null);
		_ownsCompanionProcess = false;
	}

	private void ConnectToAddress(string grpcAddress)
	{
		_channel = GrpcChannel.ForAddress(grpcAddress, CreateChannelOptions());
		_client = new CompanionService.CompanionServiceClient(_channel);
	}

	private GrpcChannelOptions CreateChannelOptions()
	{
		// Enable HTTP/2 without TLS (h2c) for gRPC
		var handler = new SocketsHttpHandler
		{
			EnableMultipleHttp2Connections = true,
			// Configure HTTP/2 settings for compatibility with Swift gRPC server
			InitialHttp2StreamWindowSize = 65535 * 16, // 1 MB
			PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
			KeepAlivePingDelay = TimeSpan.FromSeconds(60),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
		};

		return new GrpcChannelOptions
		{
			HttpHandler = handler,
			HttpVersion = new Version(2, 0),
			// Allow large messages for screenshot/video data
			MaxReceiveMessageSize = 100 * 1024 * 1024, // 100 MB
			MaxSendMessageSize = 100 * 1024 * 1024 // 100 MB
		};
	}

	private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (_client is not null && IsConnected)
			return;

		if (!_companionProcess.IsRunning && _options.TargetUdid is not null)
		{
			await _companionProcess.StartAsync(_options.TargetUdid, cancellationToken).ConfigureAwait(false);
		}

		var address = _companionProcess.GrpcAddress
			?? throw new InvalidOperationException("Companion process is not running or gRPC address is not available");

		_channel = GrpcChannel.ForAddress(address, CreateChannelOptions());
		_client = new CompanionService.CompanionServiceClient(_channel);

		_logger.LogDebug("Connected to IDB companion at {Address}", address);
	}

	private CompanionService.CompanionServiceClient GetClient()
	{
		return _client ?? throw new InvalidOperationException("Client is not connected. Call ConnectAsync first.");
	}

	private CallOptions GetCallOptions(CancellationToken cancellationToken)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cts.CancelAfter(_options.OperationTimeout);
		return new CallOptions(cancellationToken: cts.Token);
	}

	private Metadata GetMetadata() => new();

	private const int MaxRetries = 3;

	/// <summary>
	/// Executes a gRPC call with retry on PROTOCOL_ERROR.
	/// When idb_companion sends invalid HTTP/2 data, the connection is
	/// poisoned. This helper reconnects and retries the call.
	/// </summary>
	private async Task<T> WithRetryOnProtocolErrorAsync<T>(
		Func<CompanionService.CompanionServiceClient, CallOptions, Task<T>> call,
		CancellationToken cancellationToken)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		for (int attempt = 0; ; attempt++)
		{
			try
			{
				return await call(GetClient(), GetCallOptions(cancellationToken)).ConfigureAwait(false);
			}
			catch (RpcException ex) when (
				attempt < MaxRetries &&
				ex.StatusCode == StatusCode.Internal &&
				ex.Message.Contains("PROTOCOL_ERROR"))
			{
				_logger.LogDebug(
					"gRPC call failed with PROTOCOL_ERROR (attempt {Attempt}/{Max}), reconnecting",
					attempt + 1, MaxRetries);

				// Tear down the poisoned connection and create a fresh one
				await ReconnectAsync(cancellationToken).ConfigureAwait(false);
			}
		}
	}

	private async Task ReconnectAsync(CancellationToken cancellationToken)
	{
		if (_channel is not null)
		{
			try { _channel.Dispose(); } catch { /* best effort */ }
			_channel = null;
			_client = null;
		}

		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
	}

	#region Connection & Management

	/// <inheritdoc />
	public async Task<CompanionInfo> ConnectAsync(CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new ConnectRequest();
		var response = await GetClient().connectAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return new CompanionInfo
		{
			Udid = response.Companion?.Udid ?? string.Empty,
			IsLocal = response.Companion?.IsLocal ?? true
		};
	}

	/// <inheritdoc />
	public async Task<TargetDescription> DescribeAsync(bool fetchDiagnostics = false, CancellationToken cancellationToken = default)
	{
		var request = new TargetDescriptionRequest { FetchDiagnostics = fetchDiagnostics };
		var response = await WithRetryOnProtocolErrorAsync(
			(client, opts) => client.describeAsync(request, opts).ResponseAsync,
			cancellationToken).ConfigureAwait(false);

		var desc = response.TargetDescription;
		return new TargetDescription
		{
			Udid = desc?.Udid ?? string.Empty,
			Name = desc?.Name ?? string.Empty,
			State = desc?.State ?? string.Empty,
			TargetType = desc?.TargetType ?? string.Empty,
			OsVersion = desc?.OsVersion ?? string.Empty,
			Architecture = desc?.Architecture ?? string.Empty,
			Screen = desc?.ScreenDimensions is not null ? new ScreenDimensions
			{
				Width = desc.ScreenDimensions.Width,
				Height = desc.ScreenDimensions.Height,
				Density = desc.ScreenDimensions.Density,
				WidthPoints = desc.ScreenDimensions.WidthPoints,
				HeightPoints = desc.ScreenDimensions.HeightPoints
			} : null
		};
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<byte[]> LogAsync(LogSource source = LogSource.Target, IEnumerable<string>? arguments = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new LogRequest
		{
			Source = source == LogSource.Companion ? LogRequest.Types.Source.Companion : LogRequest.Types.Source.Target
		};

		if (arguments is not null)
		{
			request.Arguments.AddRange(arguments);
		}

		using var call = GetClient().log(request, GetCallOptions(cancellationToken));

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			if (response.Output.Length > 0)
			{
				yield return response.Output.ToByteArray();
			}
		}
	}

	#endregion

	#region App Lifecycle

	/// <inheritdoc />
	public async Task<InstallResult> InstallAsync(string path, InstallDestination destination = InstallDestination.App, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().install(GetCallOptions(cancellationToken));

		// Send destination
		await call.RequestStream.WriteAsync(new InstallRequest
		{
			Destination = (InstallRequest.Types.Destination)destination
		}, cancellationToken).ConfigureAwait(false);

		// Send file path payload
		await call.RequestStream.WriteAsync(new InstallRequest
		{
			Payload = new Payload { FilePath = path }
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);

		InstallResult? result = null;
		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			if (!string.IsNullOrEmpty(response.Name))
			{
				result = new InstallResult
				{
					Name = response.Name,
					Uuid = response.Uuid
				};
			}
		}

		return result ?? new InstallResult();
	}

	/// <inheritdoc />
	public async Task<InstallResult> InstallAsync(Stream stream, InstallDestination destination = InstallDestination.App, string? nameHint = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().install(GetCallOptions(cancellationToken));

		// Send destination
		await call.RequestStream.WriteAsync(new InstallRequest
		{
			Destination = (InstallRequest.Types.Destination)destination
		}, cancellationToken).ConfigureAwait(false);

		// Send name hint if provided
		if (!string.IsNullOrEmpty(nameHint))
		{
			await call.RequestStream.WriteAsync(new InstallRequest
			{
				NameHint = nameHint
			}, cancellationToken).ConfigureAwait(false);
		}

		// Send data in chunks
		var buffer = new byte[64 * 1024];
		int bytesRead;
		while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
		{
			await call.RequestStream.WriteAsync(new InstallRequest
			{
				Payload = new Payload { Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead) }
			}, cancellationToken).ConfigureAwait(false);
		}

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);

		InstallResult? result = null;
		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			if (!string.IsNullOrEmpty(response.Name))
			{
				result = new InstallResult
				{
					Name = response.Name,
					Uuid = response.Uuid
				};
			}
		}

		return result ?? new InstallResult();
	}

	/// <inheritdoc />
	public async Task UninstallAsync(string bundleId, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new UninstallRequest { BundleId = bundleId };
		await GetClient().uninstallAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<LaunchResult> LaunchAsync(string bundleId, LaunchOptions? options = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().launch(GetCallOptions(cancellationToken));

		var start = new LaunchRequest.Types.Start
		{
			BundleId = bundleId,
			ForegroundIfRunning = options?.ForegroundIfRunning ?? false,
			WaitFor = options?.WaitFor ?? false,
			WaitForDebugger = options?.WaitForDebugger ?? false
		};

		if (options?.Environment is not null)
		{
			foreach (var kvp in options.Environment)
			{
				start.Env.Add(kvp.Key, kvp.Value);
			}
		}

		if (options?.Arguments is not null)
		{
			start.AppArgs.AddRange(options.Arguments);
		}

		await call.RequestStream.WriteAsync(new LaunchRequest { Start = start }, cancellationToken).ConfigureAwait(false);
		await call.RequestStream.CompleteAsync().ConfigureAwait(false);

		var result = new LaunchResult();
		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			if (response.Debugger is not null)
			{
				result.Pid = response.Debugger.Pid;
				result.DebugHost = response.Debugger.Host;
				result.DebugPort = response.Debugger.Port;
			}
		}

		return result;
	}

	/// <inheritdoc />
	public async Task TerminateAsync(string bundleId, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new TerminateRequest { BundleId = bundleId };
		await GetClient().terminateAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<InstalledApp>> ListAppsAsync(bool suppressProcessState = false, CancellationToken cancellationToken = default)
	{
		var request = new ListAppsRequest { SuppressProcessState = suppressProcessState };
		var response = await WithRetryOnProtocolErrorAsync(
			(client, opts) => client.list_appsAsync(request, opts).ResponseAsync,
			cancellationToken).ConfigureAwait(false);

		return response.Apps.Select(app => new InstalledApp
		{
			BundleId = app.BundleId,
			Name = app.Name,
			Architectures = app.Architectures.ToList(),
			InstallType = app.InstallType,
			ProcessState = (AppProcessState)app.ProcessState,
			Debuggable = app.Debuggable,
			ProcessIdentifier = app.ProcessIdentifier
		}).ToList();
	}

	#endregion

	#region Media & Screenshots

	/// <inheritdoc />
	/// <remarks>
	/// Due to a known HTTP/2 compatibility issue between .NET's gRPC client and Swift gRPC servers,
	/// this method uses xcrun simctl as a fallback when the gRPC call fails for simulators.
	/// </remarks>
	public async Task<Screenshot> ScreenshotAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var request = new ScreenshotRequest();
			var response = await WithRetryOnProtocolErrorAsync(
				(client, opts) => client.screenshotAsync(request, opts).ResponseAsync,
				cancellationToken).ConfigureAwait(false);

			return new Screenshot
			{
				ImageData = response.ImageData.ToByteArray(),
				ImageFormat = response.ImageFormat
			};
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Internal && 
			ex.Message.Contains("PROTOCOL_ERROR"))
		{
			// Fallback to simctl for simulators if all gRPC retries exhausted
			_logger.LogDebug("gRPC screenshot failed with PROTOCOL_ERROR after retries, falling back to simctl");
			return await ScreenshotViaSimctlAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task<Screenshot> ScreenshotViaSimctlAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(TargetUdid))
			throw new InvalidOperationException("Target UDID is not set");

		var tempFile = Path.Combine(Path.GetTempPath(), $"idb_screenshot_{Guid.NewGuid():N}.png");
		try
		{
			var startInfo = new System.Diagnostics.ProcessStartInfo
			{
				FileName = "xcrun",
				ArgumentList = { "simctl", "io", TargetUdid, "screenshot", tempFile },
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			using var process = System.Diagnostics.Process.Start(startInfo);
			if (process == null)
				throw new InvalidOperationException("Failed to start simctl process");

			await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

			if (process.ExitCode != 0)
			{
				var stderr = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
				throw new InvalidOperationException($"simctl screenshot failed: {stderr}");
			}

			var imageData = await File.ReadAllBytesAsync(tempFile, cancellationToken).ConfigureAwait(false);
			return new Screenshot
			{
				ImageData = imageData,
				ImageFormat = "png"
			};
		}
		finally
		{
			try
			{
				File.Delete(tempFile);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to delete temporary screenshot file {TempFile}", tempFile);
			}
		}
	}

	/// <inheritdoc />
	public async Task AddMediaAsync(string filePath, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().add_media(GetCallOptions(cancellationToken));

		await call.RequestStream.WriteAsync(new AddMediaRequest
		{
			Payload = new Payload { FilePath = filePath }
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task AddMediaAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().add_media(GetCallOptions(cancellationToken));

		var buffer = new byte[64 * 1024];
		int bytesRead;
		while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
		{
			await call.RequestStream.WriteAsync(new AddMediaRequest
			{
				Payload = new Payload { Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead) }
			}, cancellationToken).ConfigureAwait(false);
		}

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	#endregion

	#region Interaction (HID)

	/// <inheritdoc />
	public Task TapAsync(double x, double y, CancellationToken cancellationToken = default)
		=> TapAsync(new Point(x, y), cancellationToken);

	/// <inheritdoc />
	public async Task TapAsync(Point point, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().hid(GetCallOptions(cancellationToken));

		var grpcPoint = new GrpcPoint { X = point.X, Y = point.Y };

		// Press down
		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Press = new HIDEvent.Types.HIDPress
			{
				Action = new HIDEvent.Types.HIDPressAction
				{
					Touch = new HIDEvent.Types.HIDTouch { Point = grpcPoint }
				},
				Direction = HIDEvent.Types.HIDDirection.Down
			}
		}, cancellationToken).ConfigureAwait(false);

		// Press up
		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Press = new HIDEvent.Types.HIDPress
			{
				Action = new HIDEvent.Types.HIDPressAction
				{
					Touch = new HIDEvent.Types.HIDTouch { Point = grpcPoint }
				},
				Direction = HIDEvent.Types.HIDDirection.Up
			}
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task SwipeAsync(Point start, Point end, double duration = 0.5, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().hid(GetCallOptions(cancellationToken));

		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Swipe = new HIDEvent.Types.HIDSwipe
			{
				Start = new GrpcPoint { X = start.X, Y = start.Y },
				End = new GrpcPoint { X = end.X, Y = end.Y },
				Duration = duration
			}
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task PressButtonAsync(HardwareButton button, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().hid(GetCallOptions(cancellationToken));

		var grpcButton = new HIDEvent.Types.HIDButton { Button = (HIDEvent.Types.HIDButtonType)button };

		// Press down
		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Press = new HIDEvent.Types.HIDPress
			{
				Action = new HIDEvent.Types.HIDPressAction { Button = grpcButton },
				Direction = HIDEvent.Types.HIDDirection.Down
			}
		}, cancellationToken).ConfigureAwait(false);

		// Press up
		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Press = new HIDEvent.Types.HIDPress
			{
				Action = new HIDEvent.Types.HIDPressAction { Button = grpcButton },
				Direction = HIDEvent.Types.HIDDirection.Up
			}
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task SendKeyAsync(ulong keycode, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().hid(GetCallOptions(cancellationToken));

		var key = new HIDEvent.Types.HIDKey { Keycode = keycode };

		// Press down
		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Press = new HIDEvent.Types.HIDPress
			{
				Action = new HIDEvent.Types.HIDPressAction { Key = key },
				Direction = HIDEvent.Types.HIDDirection.Down
			}
		}, cancellationToken).ConfigureAwait(false);

		// Press up
		await call.RequestStream.WriteAsync(new HIDEvent
		{
			Press = new HIDEvent.Types.HIDPress
			{
				Action = new HIDEvent.Types.HIDPressAction { Key = key },
				Direction = HIDEvent.Types.HIDDirection.Up
			}
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task FocusAsync(CancellationToken cancellationToken = default)
	{
		var request = new FocusRequest();
		await WithRetryOnProtocolErrorAsync(
			(client, opts) => client.focusAsync(request, opts).ResponseAsync,
			cancellationToken).ConfigureAwait(false);
	}

	#endregion

	#region Settings & Permissions

	/// <inheritdoc />
	public async Task ApprovePermissionAsync(string bundleId, IEnumerable<Permission> permissions, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new ApproveRequest { BundleId = bundleId };
		request.Permissions.AddRange(permissions.Select(p => (ApproveRequest.Types.Permission)p));

		await GetClient().approveAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task RevokePermissionAsync(string bundleId, IEnumerable<Permission> permissions, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new RevokeRequest { BundleId = bundleId };
		request.Permissions.AddRange(permissions.Select(p => (RevokeRequest.Types.Permission)p));

		await GetClient().revokeAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task SetSettingAsync(string name, string value, string? domain = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new SettingRequest
		{
			StringSetting = new SettingRequest.Types.StringSetting
			{
				Setting = GrpcSetting.Any,
				Name = name,
				Value = value,
				Domain = domain ?? string.Empty
			}
		};

		await GetClient().settingAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<string> GetSettingAsync(string name, string? domain = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new GetSettingRequest
		{
			Setting = GrpcSetting.Any,
			Name = name,
			Domain = domain ?? string.Empty
		};

		var response = await GetClient().get_settingAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
		return response.Value;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<string>> ListSettingsAsync(CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new ListSettingRequest { Setting = GrpcSetting.Any };
		var response = await GetClient().list_settingsAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return response.Values.ToList();
	}

	/// <inheritdoc />
	public async Task SetHardwareKeyboardAsync(bool enabled, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new SettingRequest
		{
			HardwareKeyboard = new SettingRequest.Types.HardwareKeyboard { Enabled = enabled }
		};

		await GetClient().settingAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task ClearKeychainAsync(CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new ClearKeychainRequest();
		await GetClient().clear_keychainAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public Task SetLocationAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
		=> SetLocationAsync(new GeoLocation(latitude, longitude), cancellationToken);

	/// <inheritdoc />
	public async Task SetLocationAsync(GeoLocation location, CancellationToken cancellationToken = default)
	{
		var request = new SetLocationRequest
		{
			Location = new Location
			{
				Latitude = location.Latitude,
				Longitude = location.Longitude
			}
		};

		await WithRetryOnProtocolErrorAsync(
			(client, opts) => client.set_locationAsync(request, opts).ResponseAsync,
			cancellationToken).ConfigureAwait(false);
	}

	#endregion

	#region Contacts & Data

	/// <inheritdoc />
	public async Task UpdateContactsAsync(string filePath, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new ContactsUpdateRequest
		{
			Payload = new Payload { FilePath = filePath }
		};

		await GetClient().contacts_updateAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task ClearContactsAsync(CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new ContactsClearRequest();
		await GetClient().contacts_clearAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task ClearPhotosAsync(CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new PhotosClearRequest();
		await GetClient().photos_clearAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	#endregion

	#region Accessibility

	/// <inheritdoc />
	public async Task<AccessibilityInfo> GetAccessibilityInfoAsync(Point? point = null, AccessibilityFormat format = AccessibilityFormat.Nested, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new AccessibilityInfoRequest
		{
			Format = (AccessibilityInfoRequest.Types.Format)format
		};

		if (point is not null)
		{
			request.Point = new GrpcPoint { X = point.X, Y = point.Y };
		}

		var response = await GetClient().accessibility_infoAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return new AccessibilityInfo { Json = response.Json };
	}

	#endregion

	#region File Operations

	private static GrpcFileContainer ToGrpcContainer(FileContainer? container)
	{
		if (container is null)
			return new GrpcFileContainer { Kind = GrpcFileContainer.Types.Kind.None };

		return new GrpcFileContainer
		{
			Kind = (GrpcFileContainer.Types.Kind)container.Kind,
			BundleId = container.BundleId ?? string.Empty
		};
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<IdbFileInfo>> ListFilesAsync(string path, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new LsRequest
		{
			Path = path,
			Container = ToGrpcContainer(container)
		};

		var response = await GetClient().lsAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return response.Files.Select(f => new IdbFileInfo { Path = f.Path }).ToList();
	}

	/// <inheritdoc />
	public async Task MakeDirAsync(string path, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new MkdirRequest
		{
			Path = path,
			Container = ToGrpcContainer(container)
		};

		await GetClient().mkdirAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task MoveAsync(IEnumerable<string> sourcePaths, string destinationPath, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new MvRequest
		{
			DstPath = destinationPath,
			Container = ToGrpcContainer(container)
		};
		request.SrcPaths.AddRange(sourcePaths);

		await GetClient().mvAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task RemoveAsync(IEnumerable<string> paths, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new RmRequest
		{
			Container = ToGrpcContainer(container)
		};
		request.Paths.AddRange(paths);

		await GetClient().rmAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task PushAsync(string localPath, string remotePath, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().push(GetCallOptions(cancellationToken));

		// Send inner request with destination info
		await call.RequestStream.WriteAsync(new PushRequest
		{
			Inner = new PushRequest.Types.Inner
			{
				DstPath = remotePath,
				Container = ToGrpcContainer(container)
			}
		}, cancellationToken).ConfigureAwait(false);

		// Send file payload
		await call.RequestStream.WriteAsync(new PushRequest
		{
			Payload = new Payload { FilePath = localPath }
		}, cancellationToken).ConfigureAwait(false);

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task PushAsync(Stream stream, string remotePath, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		using var call = GetClient().push(GetCallOptions(cancellationToken));

		// Send inner request with destination info
		await call.RequestStream.WriteAsync(new PushRequest
		{
			Inner = new PushRequest.Types.Inner
			{
				DstPath = remotePath,
				Container = ToGrpcContainer(container)
			}
		}, cancellationToken).ConfigureAwait(false);

		// Send data in chunks
		var buffer = new byte[64 * 1024];
		int bytesRead;
		while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
		{
			await call.RequestStream.WriteAsync(new PushRequest
			{
				Payload = new Payload { Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead) }
			}, cancellationToken).ConfigureAwait(false);
		}

		await call.RequestStream.CompleteAsync().ConfigureAwait(false);
		await call.ResponseAsync.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task PullAsync(string remotePath, string localPath, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		var data = await PullAsync(remotePath, container, cancellationToken).ConfigureAwait(false);
		await File.WriteAllBytesAsync(localPath, data, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<byte[]> PullAsync(string remotePath, FileContainer? container = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new PullRequest
		{
			SrcPath = remotePath,
			Container = ToGrpcContainer(container)
		};

		using var call = GetClient().pull(request, GetCallOptions(cancellationToken));

		using var ms = new MemoryStream();
		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			if (response.Payload?.Data is not null)
			{
				await ms.WriteAsync(response.Payload.Data.Memory, cancellationToken).ConfigureAwait(false);
			}
		}

		return ms.ToArray();
	}

	#endregion

	#region Crash Logs

	private static CrashLogQuery ToCrashQuery(string? bundleId, DateTime? since, DateTime? before, string? name = null)
	{
		var query = new CrashLogQuery();

		if (!string.IsNullOrEmpty(bundleId))
			query.BundleId = bundleId;

		if (!string.IsNullOrEmpty(name))
			query.Name = name;

		if (since.HasValue)
			query.Since = (ulong)new DateTimeOffset(since.Value).ToUnixTimeSeconds();

		if (before.HasValue)
			query.Before = (ulong)new DateTimeOffset(before.Value).ToUnixTimeSeconds();

		return query;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<CrashLogInfo>> ListCrashLogsAsync(string? bundleId = null, DateTime? since = null, DateTime? before = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var query = ToCrashQuery(bundleId, since, before);
		var response = await GetClient().crash_listAsync(query, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return response.List.Select(c => new CrashLogInfo
		{
			Name = c.Name,
			BundleId = c.BundleId,
			ProcessName = c.ProcessName,
			ParentProcessName = c.ParentProcessName,
			ProcessIdentifier = c.ProcessIdentifier,
			ParentProcessIdentifier = c.ParentProcessIdentifier,
			Timestamp = c.Timestamp
		}).ToList();
	}

	/// <inheritdoc />
	public async Task<CrashLog> GetCrashLogAsync(string name, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new CrashShowRequest { Name = name };
		var response = await GetClient().crash_showAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return new CrashLog
		{
			Name = response.Info?.Name ?? string.Empty,
			BundleId = response.Info?.BundleId ?? string.Empty,
			ProcessName = response.Info?.ProcessName ?? string.Empty,
			ParentProcessName = response.Info?.ParentProcessName ?? string.Empty,
			ProcessIdentifier = response.Info?.ProcessIdentifier ?? 0,
			ParentProcessIdentifier = response.Info?.ParentProcessIdentifier ?? 0,
			Timestamp = response.Info?.Timestamp ?? 0,
			Contents = response.Contents
		};
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<CrashLogInfo>> DeleteCrashLogsAsync(string? bundleId = null, DateTime? since = null, DateTime? before = null, string? name = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var query = ToCrashQuery(bundleId, since, before, name);
		var response = await GetClient().crash_deleteAsync(query, GetCallOptions(cancellationToken)).ConfigureAwait(false);

		return response.List.Select(c => new CrashLogInfo
		{
			Name = c.Name,
			BundleId = c.BundleId,
			ProcessName = c.ProcessName,
			ParentProcessName = c.ParentProcessName,
			ProcessIdentifier = c.ProcessIdentifier,
			ParentProcessIdentifier = c.ParentProcessIdentifier,
			Timestamp = c.Timestamp
		}).ToList();
	}

	#endregion

	#region XCTest

	/// <inheritdoc />
	public async Task<IReadOnlyList<TestBundle>> ListTestBundlesAsync(CancellationToken cancellationToken = default)
	{
		var request = new XctestListBundlesRequest();
		var response = await WithRetryOnProtocolErrorAsync(
			(client, opts) => client.xctest_list_bundlesAsync(request, opts).ResponseAsync,
			cancellationToken).ConfigureAwait(false);

		return response.Bundles.Select(b => new TestBundle
		{
			Name = b.Name,
			BundleId = b.BundleId,
			Architectures = b.Architectures.ToList()
		}).ToList();
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<string>> ListTestsAsync(string bundleName, string? appPath = null, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new XctestListTestsRequest
		{
			BundleName = bundleName,
			AppPath = appPath ?? string.Empty
		};

		var response = await GetClient().xctest_list_testsAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
		return response.Names.ToList();
	}

	/// <inheritdoc />
	public async Task<XctestRunResult> RunTestsAsync(XctestRunRequest request, CancellationToken cancellationToken = default)
	{
		var results = new List<TestRunInfo>();
		var logOutput = new List<string>();
		XctestRunStatus status = XctestRunStatus.Running;
		string? coverageJson = null;

		await foreach (var result in RunTestsStreamAsync(request, cancellationToken).ConfigureAwait(false))
		{
			results.Add(result);
		}

		// The last status would be set by the stream
		if (results.Count > 0)
		{
			status = results.Any(r => r.Status == TestStatus.Failed || r.Status == TestStatus.Crashed)
				? XctestRunStatus.TerminatedAbnormally
				: XctestRunStatus.TerminatedNormally;
		}

		return new XctestRunResult
		{
			Status = status,
			Results = results,
			LogOutput = logOutput,
			CoverageJson = coverageJson
		};
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<TestRunInfo> RunTestsStreamAsync(XctestRunRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var grpcRequest = new Idb.XctestRunRequest
		{
			TestBundleId = request.TestBundleId,
			Timeout = request.Timeout,
			ReportActivities = request.ReportActivities,
			CollectCoverage = request.CollectCoverage,
			ReportAttachments = request.ReportAttachments,
			CollectLogs = request.CollectLogs,
			WaitForDebugger = request.WaitForDebugger
		};

		// Set mode
		grpcRequest.Mode = request.Mode switch
		{
			XctestMode.Logic => new Idb.XctestRunRequest.Types.Mode { Logic = new Idb.XctestRunRequest.Types.Logic() },
			XctestMode.Application app => new Idb.XctestRunRequest.Types.Mode
			{
				Application = new Idb.XctestRunRequest.Types.Application { AppBundleId = app.AppBundleId }
			},
			XctestMode.UI ui => new Idb.XctestRunRequest.Types.Mode
			{
				Ui = new Idb.XctestRunRequest.Types.UI
				{
					AppBundleId = ui.AppBundleId,
					TestHostAppBundleId = ui.TestHostAppBundleId
				}
			},
			_ => new Idb.XctestRunRequest.Types.Mode { Logic = new Idb.XctestRunRequest.Types.Logic() }
		};

		grpcRequest.TestsToRun.AddRange(request.TestsToRun);
		grpcRequest.TestsToSkip.AddRange(request.TestsToSkip);
		grpcRequest.Arguments.AddRange(request.Arguments);

		foreach (var kvp in request.Environment)
		{
			grpcRequest.Environment.Add(kvp.Key, kvp.Value);
		}

		using var call = GetClient().xctest_run(grpcRequest, GetCallOptions(cancellationToken));

		await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		{
			foreach (var result in response.Results)
			{
				yield return new TestRunInfo
				{
					Status = (TestStatus)result.Status,
					BundleName = result.BundleName,
					ClassName = result.ClassName,
					MethodName = result.MethodName,
					Duration = result.Duration,
					FailureInfo = result.FailureInfo is not null ? new TestFailureInfo
					{
						FailureMessage = result.FailureInfo.FailureMessage,
						File = result.FailureInfo.File,
						Line = result.FailureInfo.Line
					} : null,
					Logs = result.Logs.ToList(),
					ActivityLogs = result.ActivityLogs.Select(MapActivity).ToList()
				};
			}
		}
	}

	private static TestActivity MapActivity(Idb.XctestRunResponse.Types.TestRunInfo.Types.TestActivity activity)
	{
		return new TestActivity
		{
			Title = activity.Title,
			Duration = activity.Duration,
			Uuid = activity.Uuid,
			ActivityType = activity.ActivityType,
			Name = activity.Name,
			SubActivities = activity.SubActivities.Select(MapActivity).ToList()
		};
	}

	#endregion

	#region Notifications & Misc

	/// <inheritdoc />
	public async Task SendNotificationAsync(string bundleId, string jsonPayload, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new SendNotificationRequest
		{
			BundleId = bundleId,
			JsonPayload = jsonPayload
		};

		await GetClient().send_notificationAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task OpenUrlAsync(string url, CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new OpenUrlRequest { Url = url };
		await GetClient().open_urlAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task SimulateMemoryWarningAsync(CancellationToken cancellationToken = default)
	{
		await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

		var request = new SimulateMemoryWarningRequest();
		await GetClient().simulate_memory_warningAsync(request, GetCallOptions(cancellationToken)).ConfigureAwait(false);
	}

	#endregion

	#region Dispose

	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		_channel?.Dispose();

		if (_ownsCompanionProcess)
		{
			_companionProcess.Dispose();
		}
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;
		_disposed = true;

		_channel?.Dispose();

		if (_ownsCompanionProcess)
		{
			await _companionProcess.DisposeAsync().ConfigureAwait(false);
		}
	}

	#endregion
}
