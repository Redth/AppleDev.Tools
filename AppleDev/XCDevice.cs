using CliWrap;
using CliWrap.Buffered;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace AppleDev;

public partial class XCDevice : XCRun
{
	public async Task<FileInfo> LocateAsync(CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcode = new Xcode();

		var xcodePath = await xcode.LocateAsync(cancellationToken).ConfigureAwait(false);

		if (xcodePath is null)
			throw new DirectoryNotFoundException("Xcode.app");

		var xcdevice = new FileInfo(Path.Combine(xcodePath.FullName, "Contents/Developer/usr/bin/xcdevice"));

		if (!xcdevice.Exists)
			throw new FileNotFoundException(xcdevice.FullName);

		return xcdevice;
	}

	string TypeToFlag(XCDeviceType type)
		=> type switch
		{
			XCDeviceType.Usb => "--usb",
			XCDeviceType.Wifi => "--wifi",
			_ => "--both"
		};


	async Task<IReadOnlyList<Device>> RunAsync(CancellationToken cancellationToken, string args)
	{
		var stdout = new StringBuilder();
		
		var devices = new List<Device>();
		
		var xcdevicePath = await LocateAsync(cancellationToken).ConfigureAwait(false);

		await Cli.Wrap(xcdevicePath.FullName)
			.WithArguments(args)
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			devices = JsonConvert.DeserializeObject<List<Device>>(stdout.ToString());
		}
		catch { }
	
		return devices ?? new List<Device>();
	}

	public Task<IReadOnlyList<Device>> GetDevicesAsync(CancellationToken cancellationToken = default, TimeSpan timeout = default)
	{
		base.ThrowIfNotMacOS();

		var args = $"list";

		// For the future 'us' of the future
		// The timeout ignores fractional seconds, so let's format it as whole seconds
		// for example, 0.1 is treated simply as 0
		// By default the wait is at least several seconds as xcdevice tries to
		// find network / wifi devices for debug targets
		// If you want to wait, that's great, but if you want to return quickly
		// a list of locally connected devices, or more importantly, simulators
		// then you probably want the minimal timeout which appears to be 1 second
		// If you specify 0 it seems to only sporadically return simulators
		if (timeout > TimeSpan.Zero)
			args += $" --timeout={timeout.TotalSeconds:0}";

		return RunAsync(cancellationToken, args);
	}

	public delegate Task DeviceChangeDelegate(string id, bool added);

	public async Task ObserveAsync(CancellationToken cancellationToken, XCDeviceType type, DeviceChangeDelegate handler, Func<string, Task>? consoleOutputHandler = null)
	{
		base.ThrowIfNotMacOS();

		var xcdevicePath = await LocateAsync(cancellationToken).ConfigureAwait(false);

		var args = $"observe {TypeToFlag(type)}";

		try
		{
			await Cli.Wrap(xcdevicePath.FullName)
				.WithArguments(args)
				.WithValidation(CommandResultValidation.None)
				.WithStandardErrorPipe(PipeTarget.ToDelegate(line => { consoleOutputHandler?.Invoke(line); }))
				.WithStandardOutputPipe(PipeTarget.ToDelegate(async (line) =>
				{
					consoleOutputHandler?.Invoke(line);

					string? id = null;

					if (line.StartsWith("Attach:", StringComparison.OrdinalIgnoreCase))
					{
						id = line?.Substring(7)?.Trim() ?? string.Empty;
						if (!string.IsNullOrEmpty(id) && handler is not null)
							await handler(id, true);
					}
					else if (line.StartsWith("Detach:", StringComparison.OrdinalIgnoreCase))
					{
						id = line?.Substring(7)?.Trim() ?? string.Empty;
						if (!string.IsNullOrEmpty(id) && handler is not null)
							await handler(id, false);
					}
				}))
				.ExecuteAsync(default, gracefulCancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
		}
	}
}
