﻿using CliWrap;
using CliWrap.Builders;
using CliWrap.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

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

			// For debugging purposes, save the output to a file
			Logger?.LogDebug("SimCtl listapps output for {Target}: {Output}", target, jsonOutput);

			// Parse as JSON dictionary and return the values directly
			var appsDict = JsonConvert.DeserializeObject<Dictionary<string, SimCtlApp>>(jsonOutput);
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
			var dict = JsonConvert.DeserializeObject<Dictionary<string, T>>(output);
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
		try
		{
			var output = await GetLogsInternalAsync(target, predicate, start, "json", cancellationToken).ConfigureAwait(false);

			var allLogs = string.Concat(output);

			var logs = JsonConvert.DeserializeObject<List<SimCtlLogEntry>>(allLogs);
			logs ??= new List<SimCtlLogEntry>();

			return logs;
		}
		catch (Exception ex)
		{
			Logger?.LogError("Error deserializing logs: {Exception}", ex.Message);
			return new List<SimCtlLogEntry>();
		}
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
				Logger?.LogWarning("Failed to get apps for target {Target}", target);
			}

			return output;
		}
		catch (Exception ex)
		{
			Logger?.LogWarning("ShowLogsAsync failed: {Exception}", ex.Message);
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
				.WithArguments(args =>
				{
					args.Add("simctl");
					argsBuilder?.Invoke(args);
				})
				.ExecuteAsync(cancellationToken).ConfigureAwait(false);

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

public class SimCtlLogEntry
{
    [JsonProperty("timezoneName")]
    public string TimezoneName { get; set; } = string.Empty;

    [JsonProperty("messageType")]
    public string MessageType { get; set; } = string.Empty;

    [JsonProperty("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonProperty("source")]
    public string? Source { get; set; }

    [JsonProperty("formatString")]
    public string FormatString { get; set; } = string.Empty;

    [JsonProperty("userID")]
    public long UserID { get; set; }

    [JsonProperty("activityIdentifier")]
    public long ActivityIdentifier { get; set; }

    [JsonProperty("subsystem")]
    public string Subsystem { get; set; } = string.Empty;

    [JsonProperty("category")]
    public string Category { get; set; } = string.Empty;

    [JsonProperty("threadID")]
    public long ThreadID { get; set; }

    [JsonProperty("senderImageUUID")]
    public string SenderImageUUID { get; set; } = string.Empty;

    [JsonProperty("backtrace")]
    public SimCtlLogEntryBacktrace? Backtrace { get; set; }

    [JsonProperty("bootUUID")]
    public string BootUUID { get; set; } = string.Empty;

    [JsonProperty("processImagePath")]
    public string ProcessImagePath { get; set; } = string.Empty;

    [JsonProperty("senderImagePath")]
    public string SenderImagePath { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonProperty("machTimestamp")]
    public long MachTimestamp { get; set; }

    [JsonProperty("eventMessage")]
    public string EventMessage { get; set; } = string.Empty;

    [JsonProperty("processImageUUID")]
    public string ProcessImageUUID { get; set; } = string.Empty;

    [JsonProperty("traceID")]
    public string TraceID { get; set; } = string.Empty;

    [JsonProperty("processID")]
    public long ProcessID { get; set; }

    [JsonProperty("senderProgramCounter")]
    public long SenderProgramCounter { get; set; }

    [JsonProperty("parentActivityIdentifier")]
    public long ParentActivityIdentifier { get; set; }
}

public class SimCtlLogEntryBacktrace
{
    [JsonProperty("frames")]
    public List<SimCtlLogEntryBacktraceFrame> Frames { get; set; } = new List<SimCtlLogEntryBacktraceFrame>();
}

public class SimCtlLogEntryBacktraceFrame
{
	[JsonProperty("imageOffset")]
	public long ImageOffset { get; set; }

	[JsonProperty("imageUUID")]
	public string ImageUUID { get; set; } = string.Empty;
}
