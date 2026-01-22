using CliWrap;
using CliWrap.Builders;
using CliWrap.Exceptions;
using Claunia.PropertyList;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppleDev;

public class SimCtl : XCRun
{
	readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new FlexibleStringConverter() }
	};

	public SimCtl() : base() { }

	public SimCtl(ILogger<SimCtl> logger) : base(logger) { }

	public async Task<DirectoryInfo?> LocateSimulatorAppAsync(CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var xcode = new Xcode();
		var xcodePath = await xcode.LocateAsync(cancellationToken).ConfigureAwait(false);

		if (xcodePath is null || !xcodePath.Exists)
			return null;

		var simAppPath = new DirectoryInfo(Path.Combine(xcodePath.FullName, "Contents", "Developer", "Applications", "Simulator.app"));
		if (simAppPath.Exists)
			return simAppPath;
		return null;
	}

	/// <summary>
	/// Opens the Simulator.app, optionally to a specific simulator UDID
	/// </summary>
	/// <param name="udid"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="FileNotFoundException"></exception>
	public async Task<bool> OpenSimulatorAppAsync(string? udid = null, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var simAppPath = await LocateSimulatorAppAsync(cancellationToken).ConfigureAwait(false);

		if (simAppPath is null || !simAppPath.Exists)
		{
			var errorMsg = simAppPath?.FullName ?? "Simulator.app";
			Logger?.LogError("Simulator.app not found at {Path}", errorMsg);
			throw new FileNotFoundException(errorMsg);
		}

		try
		{
			Logger?.LogDebug("Opening Simulator.app at {Path}{Target}", simAppPath.FullName, 
				string.IsNullOrEmpty(udid) ? "" : $" with UDID {udid}");

			var result = await Cli.Wrap("/usr/bin/open")
				.WithArguments(args =>
				{
					args.Add(simAppPath.FullName);

					// Open to a specific UDID ?
					if (!string.IsNullOrEmpty(udid))
					{
						args.Add("--args");
						args.Add("-CurrentDeviceUDID");
						args.Add(udid);
					}
				})
				.ExecuteAsync(cancellationToken)
				.ConfigureAwait(false);

			var success = result.ExitCode == 0;
			if (!success)
			{
				Logger?.LogWarning("Failed to open Simulator.app, exit code: {ExitCode}", result.ExitCode);
			}

			return success;
		}
		catch (Exception ex)
		{
			Logger?.LogError("Error opening Simulator.app: {Exception}", ex.Message);
			return false;
		}
	}
	
	/// <summary>
	/// Deletes the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'unavailable')</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> DeleteAsync(string target = "unavailable", CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("delete");
			args.Add(target);
		}, cancellationToken);

	/// <summary>
	/// Boots the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> BootAsync(string target, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("boot");
			args.Add(target);
		}, cancellationToken);

	/// <summary>
	/// Waits for the simulator to be ready (Booted).
	/// </summary>
	/// <param name="target">The target UDID or Simulator Name to wait for boot complete.</param>
	/// <param name="timeout">Timeout to wait for booted state.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if the simulator was found to be ready.</returns>
	public async Task<bool> WaitForBootedAsync(string target, TimeSpan timeout, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cts.CancelAfter(timeout);
		
		try
		{
			var (success, _) = await RunSimCtlCmdWithOutputAsync(args =>
			{
				args.Add("bootstatus");
				args.Add(target);
			}, cts.Token).ConfigureAwait(false);

			return success;
		}
		catch (OperationCanceledException)
		{
			Logger?.LogDebug("WaitForBootedAsync timed out waiting for {Target}", target);
			return false;
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("WaitForBootedAsync failed for {Target}: {Exception}", target, ex.Message);
			return false;
		}
	}

	/// <summary>
	/// Shuts down the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> ShutdownAsync(string target = "all", CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("shutdown");
			args.Add(target);
		}, cancellationToken);
	
	/// <summary>
	/// Erases the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> EraseAsync(string target, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("erase");
			args.Add(target);
		}, cancellationToken);
	
	/// <summary>
	/// Adds media to the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="mediaFile">The media file to add to the simulator.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> AddMediaAsync(string target, FileInfo mediaFile, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("addmedia");
			args.Add(target);
			args.Add(mediaFile.FullName);
		}, cancellationToken);

	/// <summary>
	/// Installs app to the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="app">The app's .app file/directory to add to install.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> InstallAppAsync(string target, DirectoryInfo app, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("install");
			args.Add(target);
			args.Add(app.FullName);
		}, cancellationToken);
	
	/// <summary>
	/// Uninstalls app to the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="bundleIdentifier">The app's bundle identifier to add to uninstall (eg: com.company.appname).</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> UninstallAppAsync(string target, string bundleIdentifier, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("uninstall");
			args.Add(target);
			args.Add(bundleIdentifier);
		}, cancellationToken);

	/// <summary>
	/// Launches an installed app on the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="bundleIdentifier">The app's bundle identifier to add to uninstall (eg: com.company.appname).</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> LaunchAppAsync(string target, string bundleIdentifier, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("launch");
			args.Add(target);
			args.Add(bundleIdentifier);
		}, cancellationToken);
	
	/// <summary>
	/// Terminates a running app on the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="bundleIdentifier">The app's bundle identifier to add to uninstall (eg: com.company.appname).</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> TerminateAppAsync(string target, string bundleIdentifier, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("terminate");
			args.Add(target);
			args.Add(bundleIdentifier);
		}, cancellationToken);

	/// <summary>
	/// Launches an installed app on the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="url">The URL to open.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> OpenUrlAsync(string target, Uri url, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("openurl");
			args.Add(target);
			args.Add(url.ToString());
		}, cancellationToken);
	
	/// <summary>
	/// Launches an installed app on the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, Literal value 'all', or status (eg: 'available')</param>
	/// <param name="outputFile">The filename to save the screenshot to.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> RecordScreenshotAsync(string target, FileInfo outputFile, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("io");
			args.Add(target);
			args.Add("screenshot");
			args.Add(outputFile.FullName);
		}, cancellationToken);
	
	/// <summary>
	/// Creates a new simulator device.
	/// </summary>
	/// <param name="name">The name for the new simulator device.</param>
	/// <param name="deviceTypeId">The device type identifier (e.g., "com.apple.CoreSimulator.SimDeviceType.iPhone-15" or "iPhone 15").</param>
	/// <param name="runtimeId">Optional runtime identifier. If not specified, the newest compatible runtime is used.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> CreateAsync(string name, string deviceTypeId, string? runtimeId = null, CancellationToken cancellationToken = default)
		=> RunSimCtlCmdAsync(args =>
		{
			args.Add("create");
			args.Add(name);
			args.Add(deviceTypeId);
			if (!string.IsNullOrEmpty(runtimeId))
				args.Add(runtimeId);
		}, cancellationToken);

	/// <summary>
	/// Gets the list of apps installed on the target simulator(s).
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, or 'booted' for all booted simulators</param>
	/// <param name="cancellationToken"></param>
	/// <returns>List of apps installed on the simulator(s)</returns>
	public async Task<List<SimCtlApp>> GetAppsAsync(string target, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var (success, output) = await RunSimCtlCmdWithOutputAsync(args =>
		{
			args.Add("listapps");
			args.Add(target);
		}, cancellationToken).ConfigureAwait(false);

		if (!success || string.IsNullOrWhiteSpace(output))
		{
			Logger?.LogWarning("Failed to get apps for target {Target}", target);
			return new List<SimCtlApp>();
		}

		try
		{
			// Convert simctl output to JSON format
			var jsonOutput = PreprocessSimctlOutput(output);

			// Parse as JSON dictionary and return the values directly
			var appsDict = JsonSerializer.Deserialize<Dictionary<string, SimCtlApp>>(jsonOutput);
			if (appsDict == null)
			{
				Logger?.LogWarning("Failed to deserialize apps JSON for target {Target}", target);
				return new List<SimCtlApp>();
			}

			// Return the SimCtlApp objects as-is, no post-processing
			return appsDict.Values.ToList();
		}
		catch (Exception ex)
		{
			Logger?.LogError("Error processing apps list for target {Target}: {Exception}", target, ex.Message);
			return new List<SimCtlApp>();
		}
	}

	internal static string PreprocessSimctlOutput(string raw)
	{
		// Trim outer braces
		raw = raw.Trim();

		// Replace = with : only when not inside quotes
		raw = ReplaceOutsideQuotes(raw, " = ", " : ");
		raw = ReplaceOutsideQuotes(raw, "= ", ": ");
		raw = ReplaceOutsideQuotes(raw, " =", " :");
		raw = ReplaceOutsideQuotes(raw, "=", ":");

		// Replace ; with , only when not inside quotes  
		raw = ReplaceOutsideQuotes(raw, ";", ",");

		// Replace ( with [ and ) with ] for arrays
		raw = ReplaceOutsideQuotes(raw, "(", "[");
		raw = ReplaceOutsideQuotes(raw, ")", "]");

		// Add quotes around unquoted keys (words followed by colon)
		raw = System.Text.RegularExpressions.Regex.Replace(raw, @"(?<=[{,\s\n\r])(\w+(?:\.\w+)*)\s*:", "\"$1\":");

		// Add quotes to unquoted string values (more careful regex)
		// This regex looks for : followed by whitespace, then captures everything that's not already quoted
		// and doesn't start with structural characters
		raw = System.Text.RegularExpressions.Regex.Replace(raw, @":\s*([a-zA-Z][^,\n\r}\]]*?)(?=\s*[,\n\r}\]])", m =>
		{
			string value = m.Groups[1].Value.Trim();
			// Don't quote if it's a boolean, null, number, or starts with quote/brace/bracket
			if (value == "true" || value == "false" || value == "null" ||
				double.TryParse(value, out _) || 
				value.StartsWith("\"") || value.StartsWith("{") || value.StartsWith("["))
			{
				return ": " + value;
			}
			return ": \"" + value + "\"";
		});

		// Clean up any trailing commas before closing braces/brackets
		raw = System.Text.RegularExpressions.Regex.Replace(raw, @",(\s*[}\]])", "$1");

		return raw;
	}

	static string ReplaceOutsideQuotes(string input, string find, string replace)
	{
		var result = new StringBuilder();
		bool inQuotes = false;
		bool escapeNext = false;
		
		for (int i = 0; i < input.Length; i++)
		{
			char current = input[i];
			
			if (escapeNext)
			{
				result.Append(current);
				escapeNext = false;
				continue;
			}
			
			if (current == '\\')
			{
				result.Append(current);
				escapeNext = true;
				continue;
			}
			
			if (current == '"')
			{
				inQuotes = !inQuotes;
				result.Append(current);
				continue;
			}
			
			if (!inQuotes && input.Substring(i).StartsWith(find))
			{
				result.Append(replace);
				i += find.Length - 1; // Skip the rest of the found string
				continue;
			}
			
			result.Append(current);
		}
		
		return result.ToString();
	}
	
	public async Task<List<SimCtlDevice>> GetSimulatorsAsync(bool availableOnly = true, bool includeScreenInfo = false, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var results = new List<SimCtlDevice>();

		// Set is dictionary of runtime keys and device list values
		var deviceSets = await GetSimulatorDevices(cancellationToken).ConfigureAwait(false);

		var deviceTypes = await GetSimulatorDeviceTypes(includeScreenInfo, cancellationToken);
		var runtimes = await GetSimulatorRuntimes(cancellationToken).ConfigureAwait(false);

		foreach (var deviceSet in deviceSets)
		{
			var runtime = runtimes.FirstOrDefault(r => !string.IsNullOrEmpty(r.Identifier) && r.Identifier.Equals(deviceSet.Key, StringComparison.OrdinalIgnoreCase));
			
			if (runtime != null)
			{
				foreach (var d in deviceSet.Value)
				{
					// Find the device type
					d.DeviceType = deviceTypes.FirstOrDefault(dt =>
						!string.IsNullOrEmpty(dt.Identifier) && dt.Identifier.Equals(d.DeviceTypeIdentifier));
					d.Runtime = runtime;
					
					if (!availableOnly || d.IsAvailable)
					{
						results.Add(d);
					}
				}
			}
		}

		return results;
	}

	public async Task<List<SimCtlDeviceType>> GetSimulatorGroupsAsync(bool includeScreenInfo = false, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var results = new List<SimCtlDeviceType>();

		List<SimCtlDeviceType> deviceTypes = new ();
		List<SimCtlRuntime> runtimes = new ();
		Dictionary<string, List<SimCtlDevice>> deviceSets = new ();

		await Task.WhenAll(
			Task.Run(async () => deviceTypes = await GetSimulatorDeviceTypes(includeScreenInfo, cancellationToken)),
			Task.Run(async () => runtimes = await GetSimulatorRuntimes(cancellationToken)),
			Task.Run(async () => deviceSets = await GetSimulatorDevices(cancellationToken)));

		foreach (var deviceType in deviceTypes)
		{
			foreach (var deviceSet in deviceSets)
			{
				var deviceRuntimeIdentifier = deviceSet.Key;

				// Find all the devices for all the runtimes
				foreach (var device in deviceSet.Value)
				{
					if (device.IsAvailable && (device?.Name?.Equals(deviceType?.Name) ?? false))
					{
						var runtime = runtimes.FirstOrDefault(r =>
							r.IsAvailable
							&& !string.IsNullOrEmpty(r.Identifier)
							&& r.Identifier.Equals(deviceRuntimeIdentifier, StringComparison.OrdinalIgnoreCase));

						if (runtime != null)
						{
							device.Runtime = runtime;
							deviceType.Devices.Add(device);
						}
					}
				}
			}

			if (deviceType?.Devices?.Any() == true)
				results.Add(deviceType);
		}

		return results;
	}

	internal async Task<List<SimCtlDeviceType>> GetSimulatorDeviceTypes(bool includeScreenInfo = false, CancellationToken cancellationToken = default)
	{
		var deviceTypes = await WrapSimCtl<List<SimCtlDeviceType>>("devicetypes", cancellationToken).ConfigureAwait(false);
		deviceTypes ??= new List<SimCtlDeviceType>();

		if (includeScreenInfo)
		{
			var tasks = deviceTypes.Select(dt => PopulateScreenInfoAsync(dt, cancellationToken));
			await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		return deviceTypes;
	}

	async Task<List<SimCtlRuntime>> GetSimulatorRuntimes(CancellationToken cancellationToken = default)
	{
		var runtimes = await WrapSimCtl<List<SimCtlRuntime>>("runtimes", cancellationToken).ConfigureAwait(false);
		return runtimes ?? new List<SimCtlRuntime>();
	}


	async Task<Dictionary<string, List<SimCtlDevice>>> GetSimulatorDevices(CancellationToken cancellationToken = default)
	{
		var devices = await WrapSimCtl<Dictionary<string, List<SimCtlDevice>>>("devices", cancellationToken).ConfigureAwait(false);
		return devices ?? new Dictionary<string, List<SimCtlDevice>>();
	}

	async Task PopulateScreenInfoAsync(SimCtlDeviceType deviceType, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(deviceType?.BundlePath))
			return;

		var profilePath = Path.Combine(deviceType.BundlePath, "Contents", "Resources", "profile.plist");
		if (!File.Exists(profilePath))
		{
			Logger?.LogDebug("Profile.plist not found for device type: {DeviceType}", deviceType.Name);
			return;
		}

		try
		{
			var plist = PropertyListParser.Parse(profilePath);
			if (plist is NSDictionary dict)
			{
				var screen = new SimCtlScreenInfo();

				var widthNum = dict["mainScreenWidth"] as NSNumber;
				if (widthNum != null)
				{
					screen.Width = (int)widthNum;
					Logger?.LogDebug("Set screen Width to {Width} for device type {DeviceType}", screen.Width, deviceType.Name);
				}

				var heightNum = dict["mainScreenHeight"] as NSNumber;
				if (heightNum != null)
				{
					screen.Height = (int)heightNum;
					Logger?.LogDebug("Set screen Height to {Height} for device type {DeviceType}", screen.Height, deviceType.Name);
				}

				var scaleNum = dict["mainScreenScale"] as NSNumber;
				if (scaleNum != null)
				{
					screen.Scale = scaleNum.ToDouble();
					Logger?.LogDebug("Set screen Scale to {Scale} for device type {DeviceType}", screen.Scale, deviceType.Name);
				}

				var widthDPINum = dict["mainScreenWidthDPI"] as NSNumber;
				if (widthDPINum != null)
				{
					screen.WidthDPI = (int)widthDPINum;
					Logger?.LogDebug("Set screen WidthDPI to {WidthDPI} for device type {DeviceType}", screen.WidthDPI, deviceType.Name);
				}

				var heightDPINum = dict["mainScreenHeightDPI"] as NSNumber;
				if (heightDPINum != null)
				{
					screen.HeightDPI = (int)heightDPINum;
					Logger?.LogDebug("Set screen HeightDPI to {HeightDPI} for device type {DeviceType}", screen.HeightDPI, deviceType.Name);
				}

				var colorString = dict["mainScreenColorspace"] as NSString;
				if (colorString != null)
					screen.Colorspace = colorString.Content;

				var modelIdString = dict["modelIdentifier"] as NSString;
				if (modelIdString != null)
					deviceType.ModelIdentifier = modelIdString.Content;

				var productClassString = dict["productClass"] as NSString;
				if (productClassString != null)
					deviceType.ProductClass = productClassString.Content;

				deviceType.Screen = screen;
				Logger?.LogDebug("Screen info populated for device type {DeviceType}: Width={Width}, Height={Height}, Scale={Scale}", deviceType.Name, screen.Width, screen.Height, screen.Scale);
			}
			else
			{
				Logger?.LogWarning("Profile.plist is not a NSDictionary for device type: {DeviceType}", deviceType.Name);
			}
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("Failed to parse profile.plist for device type {DeviceType}: {Error}",
				deviceType?.Name, ex.Message);
		}
	}

	async Task<T?> WrapSimCtl<T>(string cmd, CancellationToken cancellationToken = default)
	{
		var (success, output) = await RunSimCtlCmdWithOutputAsync(args =>
		{
			args.Add("list");
			args.Add("-j");
			args.Add(cmd);
		}, cancellationToken).ConfigureAwait(false);

		if (!success || string.IsNullOrWhiteSpace(output))
		{
			Logger?.LogWarning("Failed to execute simctl list command for {Command}", cmd);
			return default;
		}

		try
		{
			var dict = JsonSerializer.Deserialize<Dictionary<string, T>>(output);
			dict ??= new Dictionary<string, T>();

			if (dict.TryGetValue(cmd, out var r))
				return r;

			return default;
		}
		catch (Exception ex)
		{
			Logger?.LogError("Error deserializing simctl list output for {Command}: {Exception}", cmd, ex.Message);
			return default;
		}
	}

	/// <summary>
	/// Get the plain log entries from the target simulator.
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, or 'booted' for all booted simulators</param>
	/// <param name="predicate">Filter events using the given predicate</param>
	/// <param name="start">Display events from the given start date</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Log output as a list of strings</returns>
	public Task<List<string>> GetLogsPlainAsync(string target, string? predicate = null, DateTimeOffset? start = null, CancellationToken cancellationToken = default)
		=> GetLogsInternalAsync(target, predicate, start, "compact", cancellationToken);

	/// <summary>
	/// Get the log entries from the target simulator.
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, or 'booted' for all booted simulators</param>
	/// <param name="predicate">Filter events using the given predicate</param>
	/// <param name="start">Display events from the given start date</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Log output as a list of log entries</returns>
	public async Task<List<SimCtlLogEntry>> GetLogsAsync(string target, string? predicate = null, DateTimeOffset? start = null, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var xcrun = LocateOrThrow();
		var logs = new List<SimCtlLogEntry>();

		try
		{
			// Use the stream-based deserialization for better performance
			logs = await GetLogsStreamAsync(xcrun.FullName, target, predicate, start, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger?.LogError("Error getting logs for target {Target}: {Exception}", target, ex.Message);
		}

		return logs;
	}

	async Task<List<SimCtlLogEntry>> GetLogsStreamAsync(string xcrunPath, string target, string? predicate = null, DateTimeOffset? start = null, CancellationToken cancellationToken = default)
	{
		var logs = new List<SimCtlLogEntry>();
		var jsonContent = new StringBuilder();
		var captureJson = false;
		var jsonDepth = 0;

		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = xcrunPath,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			psi.ArgumentList.Add("simctl");
			psi.ArgumentList.Add("spawn");
			psi.ArgumentList.Add(target);
			psi.ArgumentList.Add("log");
			psi.ArgumentList.Add("show");

			if (!string.IsNullOrEmpty(predicate))
			{
				psi.ArgumentList.Add("--predicate");
				psi.ArgumentList.Add(predicate);
			}

			if (start.HasValue)
			{
				psi.ArgumentList.Add("--start");
				psi.ArgumentList.Add(start.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
			}

			psi.ArgumentList.Add("--style");
			psi.ArgumentList.Add("json");
			psi.ArgumentList.Add("--color");
			psi.ArgumentList.Add("none");
			psi.ArgumentList.Add("--info");
			psi.ArgumentList.Add("--debug");

			using var process = new Process { StartInfo = psi };
			process.Start();

			// Read output and collect JSON
			using var reader = process.StandardOutput;
			string? line;
			while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
			{
				var trimmed = line.Trim();

				// Skip empty lines
				if (string.IsNullOrWhiteSpace(trimmed))
					continue;

				// Detect start of JSON (either array or object)
				if (!captureJson && (trimmed.StartsWith("[") || trimmed.StartsWith("{")))
				{
					captureJson = true;
					jsonDepth = 0;
				}

				if (captureJson)
				{
					jsonContent.AppendLine(line);

					// Track JSON depth to know when we have a complete structure
					// Need to handle strings that contain braces/brackets
					bool inString = false;
					bool escapeNext = false;

					foreach (var ch in trimmed)
					{
						if (escapeNext)
						{
							escapeNext = false;
							continue;
						}

						if (ch == '\\')
						{
							escapeNext = true;
							continue;
						}

						if (ch == '"')
						{
							inString = !inString;
							continue;
						}

						if (!inString)
						{
							if (ch == '[' || ch == '{') jsonDepth++;
							else if (ch == ']' || ch == '}') jsonDepth--;
						}
					}

					// If we've closed all brackets/braces, we have complete JSON
					if (jsonDepth == 0)
					{
						var jsonText = jsonContent.ToString();
						jsonContent.Clear();
						captureJson = false;

						// Try to deserialize as array first (most common for log show)
						try
						{
							var entries = JsonSerializer.Deserialize<List<SimCtlLogEntry>>(jsonText, _jsonOptions);
							if (entries != null)
								logs.AddRange(entries);
							continue;
						}
						catch (JsonException ex)
						{
							Logger?.LogDebug("Failed to parse JSON as array: {Error}", ex.Message);
						}

						// Try as single object
						try
						{
							var entry = JsonSerializer.Deserialize<SimCtlLogEntry>(jsonText, _jsonOptions);
							if (entry != null)
								logs.Add(entry);
						}
						catch (JsonException ex)
						{
							Logger?.LogDebug("Failed to parse JSON log entry: {Error}", ex.Message);
						}
					}
				}
			}

			await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger?.LogError("Error streaming logs for target {Target}: {Exception}", target, ex.Message);
		}

		return logs;
	}

	async Task<List<string>> GetLogsInternalAsync(string target, string? predicate = null, DateTimeOffset? start = null, string? style = null, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var xcrun = LocateOrThrow();

		try
		{
			var (success, output) = await RunLogSpawnProcessWithOutputAsync(target, args =>
			{
				args.Add("show");

				if (!string.IsNullOrEmpty(predicate))
				{
					args.Add("--predicate");
					args.Add(predicate);
				}

				if (start.HasValue)
				{
					args.Add("--start");
					args.Add(start.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
				}

				args.Add("--style");
				args.Add(style ?? "compact");

				args.Add("--color");
				args.Add("none");

				args.Add("--info");

				args.Add("--debug");
			}, cancellationToken).ConfigureAwait(false);

			if (!success || output.Count == 0)
			{
				Logger?.LogWarning("Failed to get logs for target {Target}", target);
			}

			return output;
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("GetLogsInternalAsync failed: {Exception}", ex.Message);
			return new List<string>();
		}
	}

	/// <summary>
	/// Collects system logs into a log archive from the target simulator.
	/// </summary>
	/// <param name="target">The target UDID, Simulator Name, or 'booted' for all booted simulators</param>
	/// <param name="outputPath">Output log archive to the given path</param>
	/// <param name="start">Collect logs starting at the given time</param>
	/// <param name="predicate">Collect logs using a given predicate</param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if command execution exit code is zero.</returns>
	public Task<bool> CollectLogsAsync(string target, string outputPath, DateTimeOffset? start = null, string? predicate = null, CancellationToken cancellationToken = default)
		=> RunLogSpawnProcessAsync(target, args =>
		{
			args.Add("collect");
			
			args.Add("--output");
			args.Add(outputPath);

			if (start.HasValue)
			{
				args.Add("--start");
				args.Add(start.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
			}
			
			if (!string.IsNullOrEmpty(predicate))
			{
				args.Add("--predicate");
				args.Add(predicate);
			}
		}, cancellationToken: cancellationToken);

	async Task<bool> RunLogSpawnProcessAsync(string target, Action<Collection<string>> logArgsBuilder, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = LocateOrThrow();

		try
		{
			await RunProcessWithEventsAsync(xcrun.FullName, args =>
			{
				args.Add("simctl");
				args.Add("spawn");
				args.Add(target);
				args.Add("log");
				logArgsBuilder(args);
			}, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("RunLogSpawnProcessAsync failed: {Exception}", ex.Message);
			return false;
		}
	}

	async Task<(bool Success, List<string> Output)> RunLogSpawnProcessWithOutputAsync(string target, Action<Collection<string>> logArgsBuilder, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = LocateOrThrow();

		try
		{
			var output = await RunProcessWithEventsAsync(xcrun.FullName, args =>
			{
				args.Add("simctl");
				args.Add("spawn");
				args.Add(target);
				args.Add("log");
				logArgsBuilder(args);
			}, cancellationToken).ConfigureAwait(false);

			return (true, output);
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("RunLogSpawnProcessWithOutputAsync failed: {Exception}", ex.Message);
			return (false, new List<string>());
		}
	}

	async Task<bool> RunSimCtlCmdAsync(Action<ArgumentsBuilder> argsBuilder, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = LocateOrThrow();

		try
		{
			var result = await Cli.Wrap(xcrun.FullName)
				.WithValidation(CommandResultValidation.None)
				.WithArguments(args =>
				{
					args.Add("simctl");
					argsBuilder?.Invoke(args);
				})
				.ExecuteAsync(cancellationToken)
				.ConfigureAwait(false);

			return result.ExitCode == 0;
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("RunSimCtlCmdAsync failed: {Exception}", ex.Message);
			return false;
		}
	}

	async Task<(bool Success, string Output)> RunSimCtlCmdWithOutputAsync(Action<ArgumentsBuilder> argsBuilder, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = LocateOrThrow();
		var stdout = new StringBuilder();

		try
		{
			var result = await Cli.Wrap(xcrun.FullName)
				.WithArguments(args =>
				{
					args.Add("simctl");
					argsBuilder?.Invoke(args);
				})
				.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
				.ExecuteAsync(cancellationToken).ConfigureAwait(false);

			return (result.ExitCode == 0, stdout.ToString());
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("RunSimCtlCmdWithOutputAsync failed: {Exception}", ex.Message);
			return (false, string.Empty);
		}
	}

	// Workaround for https://github.com/Tyrrrz/CliWrap/issues/297
	static async Task<List<string>> RunProcessWithEventsAsync(string fileName, Action<Collection<string>> logArgsBuilder, CancellationToken cancellationToken = default)
	{
		var outputLines = new List<string>();
		var errorLines = new List<string>();
		var outputComplete = new TaskCompletionSource<bool>();
		var errorComplete = new TaskCompletionSource<bool>();

		var psi = new ProcessStartInfo
		{
			FileName = fileName,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		logArgsBuilder(psi.ArgumentList);

		using var process = new Process { StartInfo = psi };

		process.OutputDataReceived += (sender, e) =>
		{
			if (e.Data == null)
			{
				outputComplete.SetResult(true);
				return;
			}

			outputLines.Add(e.Data);
		};

		process.ErrorDataReceived += (sender, e) =>
		{
			if (e.Data == null)
			{
				errorComplete.SetResult(true);
				return;
			}

			errorLines.Add(e.Data);
		};

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		// Wait for the process to exit
		await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

		// Wait for output streams to complete
		await Task.WhenAll(outputComplete.Task, errorComplete.Task).ConfigureAwait(false);

		if (process.ExitCode == 0)
			return outputLines;
		else
			throw new CommandExecutionException(
				new Command(fileName)
					.WithArguments(psi.ArgumentList),
				process.ExitCode,
				string.Join('\'', errorLines));
	}
}


public class SimCtlRuntime
{
	[JsonPropertyName("bundlePath")]
	public string? BundlePath { get; set; }

	[JsonPropertyName("buildVersion")]
	public string? BuildVersion { get; set; }

	[JsonPropertyName("runtimeRoot")]
	public string? RuntimeRoot { get; set; }

	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("version")]
	public string? Version { get; set; }

	[JsonPropertyName("isAvailable")]
	public bool IsAvailable { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }
}

public class SimCtlScreenInfo
{
	[JsonPropertyName("width")]
	public int Width { get; set; }

	[JsonPropertyName("height")]
	public int Height { get; set; }

	[JsonPropertyName("scale")]
	public double Scale { get; set; }

	[JsonPropertyName("widthDPI")]
	public int WidthDPI { get; set; }

	[JsonPropertyName("heightDPI")]
	public int HeightDPI { get; set; }

	[JsonPropertyName("colorspace")]
	public string? Colorspace { get; set; }

	[JsonIgnore]
	public int PixelWidth => (int)(Width * Scale);

	[JsonIgnore]
	public int PixelHeight => (int)(Height * Scale);
}

public class SimCtlDeviceType
{
	[JsonPropertyName("minRuntimeVersion")]
	public long MinRuntimeVersion { get; set; }

	[JsonPropertyName("bundlePath")]
	public string? BundlePath { get; set; }

	[JsonPropertyName("maxRuntimeVersion")]
	public long MaxRuntimeVersion { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("productFamily")]
	public string? ProductFamily { get; set; }

	[JsonPropertyName("modelIdentifier")]
	public string? ModelIdentifier { get; set; }

	[JsonPropertyName("productClass")]
	public string? ProductClass { get; set; }

	[JsonPropertyName("devices")]
	public List<SimCtlDevice> Devices { get; set; } = new List<SimCtlDevice>();

	[JsonIgnore]
	public SimCtlScreenInfo? Screen { get; set; }
}

public class SimCtlDevice
{
	[JsonPropertyName("dataPath")]
	public string? DataPath { get; set; }

	[JsonPropertyName("logPath")]
	public string? LogPath { get; set; }

	[JsonPropertyName("udid")]
	public string? Udid { get; set; }

	[JsonPropertyName("isAvailable")]
	public bool IsAvailable { get; set; }

	[JsonIgnore]
	public bool IsBooted
		=> !string.IsNullOrEmpty(State) && State.Contains("Booted", StringComparison.OrdinalIgnoreCase);

	[JsonPropertyName("deviceTypeIdentifier")]
	public string? DeviceTypeIdentifier { get; set; }

	[JsonPropertyName("state")]
	public string? State { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("availabilityError")]
	public string? AvailabilityError { get; set; }

	[JsonPropertyName("deviceType")]
	public SimCtlDeviceType? DeviceType { get; set; }

	[JsonPropertyName("runtime")]
	public SimCtlRuntime? Runtime { get; set; }
}

public class SimCtlApp
{
	[JsonPropertyName("ApplicationType")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? ApplicationType { get; set; }

	[JsonPropertyName("Bundle")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Bundle { get; set; }

	[JsonPropertyName("CFBundleDisplayName")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? CFBundleDisplayName { get; set; }

	[JsonPropertyName("CFBundleExecutable")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? CFBundleExecutable { get; set; }

	[JsonPropertyName("CFBundleIdentifier")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? CFBundleIdentifier { get; set; }

	[JsonPropertyName("CFBundleName")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? CFBundleName { get; set; }

	[JsonPropertyName("CFBundleVersion")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? CFBundleVersion { get; set; }

	[JsonPropertyName("DataContainer")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? DataContainer { get; set; }

	[JsonPropertyName("GroupContainers")]
	public Dictionary<string, string>? GroupContainers { get; set; }

	[JsonPropertyName("Path")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Path { get; set; }

	[JsonPropertyName("SBAppTags")]
	public List<string>? SBAppTags { get; set; }
}

public class SimCtlLogEntry
{
	[JsonPropertyName("timezoneName")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string TimezoneName { get; set; } = string.Empty;

	[JsonPropertyName("messageType")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string MessageType { get; set; } = string.Empty;

	[JsonPropertyName("eventType")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string EventType { get; set; } = string.Empty;

	[JsonPropertyName("source")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Source { get; set; }

	[JsonPropertyName("formatString")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string FormatString { get; set; } = string.Empty;

	[JsonPropertyName("userID")]
	public long UserID { get; set; }

	[JsonPropertyName("activityIdentifier")]
	public long ActivityIdentifier { get; set; }

	[JsonPropertyName("subsystem")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string Subsystem { get; set; } = string.Empty;

	[JsonPropertyName("category")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string Category { get; set; } = string.Empty;

	[JsonPropertyName("threadID")]
	public long ThreadID { get; set; }

	[JsonPropertyName("senderImageUUID")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string SenderImageUUID { get; set; } = string.Empty;

	[JsonPropertyName("backtrace")]
	public SimCtlLogEntryBacktrace? Backtrace { get; set; }

	[JsonPropertyName("bootUUID")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string BootUUID { get; set; } = string.Empty;

	[JsonPropertyName("processImagePath")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string ProcessImagePath { get; set; } = string.Empty;

	[JsonPropertyName("senderImagePath")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string SenderImagePath { get; set; } = string.Empty;

	[JsonPropertyName("timestamp")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string Timestamp { get; set; } = string.Empty;

	[JsonPropertyName("machTimestamp")]
	public long MachTimestamp { get; set; }

	[JsonPropertyName("eventMessage")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string EventMessage { get; set; } = string.Empty;

	[JsonPropertyName("processImageUUID")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string ProcessImageUUID { get; set; } = string.Empty;

	[JsonPropertyName("traceID")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string TraceID { get; set; } = string.Empty;

	[JsonPropertyName("processID")]
	public long ProcessID { get; set; }

	[JsonPropertyName("senderProgramCounter")]
	public long SenderProgramCounter { get; set; }

	[JsonPropertyName("parentActivityIdentifier")]
	public long ParentActivityIdentifier { get; set; }
}

public class SimCtlLogEntryBacktrace
{
	[JsonPropertyName("frames")]
	public List<SimCtlLogEntryBacktraceFrame> Frames { get; set; } = new List<SimCtlLogEntryBacktraceFrame>();
}

public class SimCtlLogEntryBacktraceFrame
{
	[JsonPropertyName("imageOffset")]
	public long ImageOffset { get; set; }

	[JsonPropertyName("imageUUID")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string ImageUUID { get; set; } = string.Empty;
}

public sealed class FlexibleStringConverter : JsonConverter<string?>
{
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return reader.GetString();
			case JsonTokenType.Null:
				return null;
			default:
				using (var doc = JsonDocument.ParseValue(ref reader))
				{
					return doc.RootElement.ToString();
				}
		}
	}

	public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value);
	}
}
