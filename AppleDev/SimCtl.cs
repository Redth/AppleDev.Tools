﻿using CliWrap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.Builders;
using Microsoft.Extensions.Logging;

namespace AppleDev;

public class SimCtl : XCRun
{
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
			throw new FileNotFoundException(simAppPath?.FullName ?? "Simulator.app");

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

		return result.ExitCode == 0;
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
		
		var xcrun = LocateOrThrow();
		var stdout = new StringBuilder();
		
		try
		{
			var result = await Cli.Wrap(xcrun.FullName)
				.WithArguments(args =>
				{
					args.Add("simctl");
					args.Add("bootstatus");
					args.Add(target);
				})
				.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
				.ExecuteAsync(cts.Token).ConfigureAwait(false);

			return result.ExitCode == 0;
		}
		catch
		{
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
	public async Task<List<SimCtlApp>> GetAppsAsync(string target = "booted", CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = LocateOrThrow();
		var stdout = new StringBuilder();

		var result = await Cli.Wrap(xcrun.FullName)
			.WithArguments(args =>
			{
				args.Add("simctl");
				args.Add("listapps");
				args.Add(target);
			})
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		if (result.ExitCode != 0)
			return new List<SimCtlApp>();

		var output = stdout.ToString();
		if (string.IsNullOrWhiteSpace(output))
			return new List<SimCtlApp>();

		// Convert simctl output to JSON format
		var jsonOutput = PreprocessSimctlOutput(output);

		File.WriteAllText("simctl_apps.json", jsonOutput); // For debugging purposes, save the output to a file
		
		// Parse as JSON dictionary and return the values directly
		var appsDict = JsonConvert.DeserializeObject<Dictionary<string, SimCtlApp>>(jsonOutput);
		if (appsDict == null)
			return new List<SimCtlApp>();

		// Return the SimCtlApp objects as-is, no post-processing
		return appsDict.Values.ToList();
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

	private static string ReplaceOutsideQuotes(string input, string find, string replace)
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

	async Task<bool> RunSimCtlCmdAsync(Action<ArgumentsBuilder> argsBuilder, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = LocateOrThrow();

		try
		{
			var result = await Cli.Wrap(xcrun.FullName)
				.WithArguments(args =>
				{
					args.Add("simctl");
					argsBuilder?.Invoke(args);
				})
				.ExecuteAsync(cancellationToken).ConfigureAwait(false);

			return result.ExitCode == 0;
		}
		catch
		{
			return false;
		}
	}

	public async Task<List<SimCtlDevice>> GetSimulatorsAsync(bool availableOnly = true, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var results = new List<SimCtlDevice>();

		// Set is dictionary of runtime keys and device list values
		var deviceSets = await GetSimulatorDevices(cancellationToken).ConfigureAwait(false);

		var deviceTypes = await GetSimulatorDeviceTypes(cancellationToken);
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

	public async Task<List<SimCtlDeviceType>> GetSimulatorGroupsAsync(CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var results = new List<SimCtlDeviceType>();

		List<SimCtlDeviceType> deviceTypes = new ();
		List<SimCtlRuntime> runtimes = new ();
		Dictionary<string, List<SimCtlDevice>> deviceSets = new ();

		await Task.WhenAll(
			Task.Run(async () => deviceTypes = await GetSimulatorDeviceTypes(cancellationToken)),
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

	async Task<List<SimCtlDeviceType>> GetSimulatorDeviceTypes(CancellationToken cancellationToken = default)
	{
		var deviceTypes = await WrapSimCtl<List<SimCtlDeviceType>>("devicetypes", cancellationToken).ConfigureAwait(false);
		return deviceTypes ?? new List<SimCtlDeviceType>();
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

	async Task<T?> WrapSimCtl<T>(string cmd, CancellationToken cancellationToken = default)
	{
		var xcrun = LocateOrThrow();
		var stdout = new StringBuilder();

		await Cli.Wrap(xcrun.FullName)
			.WithArguments(new[]
			{
				"simctl",
				"list",
				"-j",
				cmd
			})
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		var dict = JsonConvert.DeserializeObject<Dictionary<string, T>>(stdout.ToString());
		dict ??= new Dictionary<string, T>();

		if (dict.TryGetValue(cmd, out var r))
			return r;

		return default;
	}
}


public class SimCtlRuntime
{
	[JsonProperty("bundlePath")]
	public string? BundlePath { get; set; }

	[JsonProperty("buildVersion")]
	public string? BuildVersion { get; set; }

	[JsonProperty("runtimeRoot")]
	public string? RuntimeRoot { get; set; }

	[JsonProperty("identifier")]
	public string? Identifier { get; set; }

	[JsonProperty("version")]
	public string? Version { get; set; }

	[JsonProperty("isAvailable")]
	public bool IsAvailable { get; set; }

	[JsonProperty("name")]
	public string? Name { get; set; }
}

public class SimCtlDeviceType
{
	[JsonProperty("minRuntimeVersion")]
	public long MinRuntimeVersion { get; set; }

	[JsonProperty("bundlePath")]
	public string? BundlePath { get; set; }

	[JsonProperty("maxRuntimeVersion")]
	public long MaxRuntimeVersion { get; set; }

	[JsonProperty("name")]
	public string? Name { get; set; }

	[JsonProperty("identifier")]
	public string? Identifier { get; set; }

	[JsonProperty("productFamily")]
	public string? ProductFamily { get; set; }

	[JsonProperty("devices")]
	public List<SimCtlDevice> Devices { get; set; } = new List<SimCtlDevice>();
}

public class SimCtlDevice
{
	[JsonProperty("dataPath")]
	public string? DataPath { get; set; }

	[JsonProperty("logPath")]
	public string? LogPath { get; set; }

	[JsonProperty("udid")]
	public string? Udid { get; set; }

	[JsonProperty("isAvailable")]
	public bool IsAvailable { get; set; }

	[JsonIgnore]
	public bool IsBooted
		=> !string.IsNullOrEmpty(State) && State.Contains("Booted", StringComparison.OrdinalIgnoreCase);

	[JsonProperty("deviceTypeIdentifier")]
	public string? DeviceTypeIdentifier { get; set; }

	[JsonProperty("state")]
	public string? State { get; set; }

	[JsonProperty("name")]
	public string? Name { get; set; }

	[JsonProperty("availabilityError")]
	public string? AvailabilityError { get; set; }

	[JsonProperty("deviceType")]
	public SimCtlDeviceType? DeviceType { get; set; }

	[JsonProperty("runtime")]
	public SimCtlRuntime? Runtime { get; set; }
}

public class SimCtlApp
{
	[JsonProperty("ApplicationType")]
	public string? ApplicationType { get; set; }

	[JsonProperty("Bundle")]
	public string? Bundle { get; set; }

	[JsonProperty("CFBundleDisplayName")]
	public string? CFBundleDisplayName { get; set; }

	[JsonProperty("CFBundleExecutable")]
	public string? CFBundleExecutable { get; set; }

	[JsonProperty("CFBundleIdentifier")]
	public string? CFBundleIdentifier { get; set; }

	[JsonProperty("CFBundleName")]
	public string? CFBundleName { get; set; }

	[JsonProperty("CFBundleVersion")]
	public string? CFBundleVersion { get; set; }

	[JsonProperty("DataContainer")]
	public string? DataContainer { get; set; }

	[JsonProperty("GroupContainers")]
	public Dictionary<string, string>? GroupContainers { get; set; }

	[JsonProperty("Path")]
	public string? Path { get; set; }

	[JsonProperty("SBAppTags")]
	public List<string>? SBAppTags { get; set; }
}
