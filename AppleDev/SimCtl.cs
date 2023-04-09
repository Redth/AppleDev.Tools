using CliWrap;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AppleDev
{
	public class SimCtl : XCRun
	{
		public async Task<DirectoryInfo?> LocateSimulatorAppAsync(CancellationToken cancellationToken = default)
		{
			var xcode = new Xcode();
			var xcodePath = await xcode.LocateAsync(cancellationToken).ConfigureAwait(false);

			if (xcodePath is null || !xcodePath.Exists)
				return null;

			var simAppPath = new DirectoryInfo(Path.Combine(xcodePath.FullName, "Contents", "Developer", "Applications", "Simulator.app"));
			if (simAppPath.Exists)
				return simAppPath;
			return null;
		}

		public async Task<bool> OpenSimulatorAppAsync(CancellationToken cancellationToken = default)
		{
			var simAppPath = await LocateSimulatorAppAsync(cancellationToken).ConfigureAwait(false);

			if (simAppPath is null || !simAppPath.Exists)
				throw new FileNotFoundException(simAppPath?.FullName ?? "Simulator.app");

			var result = await Cli.Wrap("/usr/bin/open")
				.WithArguments(simAppPath.FullName)
				.ExecuteAsync(cancellationToken)
				.ConfigureAwait(false);

			return result.ExitCode == 0;
		}

		public async Task Boot(string uuid, CancellationToken cancellationToken = default)
		{
			var xcrun = LocateOrThrow();
			var stdout = new StringBuilder();

			await Cli.Wrap(xcrun.FullName)
				.WithArguments(new[]
				{
					"simctl",
					"boot",
					uuid
				})
				.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
				.ExecuteAsync(cancellationToken).ConfigureAwait(false);
		}

		public async Task<List<SimCtlDevice>> GetSimulatorsAsync(CancellationToken cancellationToken = default)
		{
			var results = new List<SimCtlDevice>();

			// Set is dictionary of runtime keys and device list values
			var deviceSets = await GetSimulatorDevices(cancellationToken).ConfigureAwait(false);

			//var deviceTypes = GetSimulatorDeviceTypes();
			var runtimes = await GetSimulatorRuntimes(cancellationToken).ConfigureAwait(false);

			foreach (var deviceSet in deviceSets)
			{
				var runtime = runtimes.FirstOrDefault(r => r.Identifier.Equals(deviceSet.Key, StringComparison.OrdinalIgnoreCase));

				if (runtime != null)
				{
					foreach (var d in deviceSet.Value)
					{
						if (d.IsAvailable)
						{
							d.Runtime = runtime;
							results.Add(d);
						}
					}
				}
			}

			return results;
		}

		public async Task<List<SimCtlDeviceType>> GetSimulatorGroupsAsync(CancellationToken cancellationToken = default)
		{
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
						if (device.IsAvailable && device.Name.Equals(deviceType.Name))
						{
							var runtime = runtimes.FirstOrDefault(r =>
								r.IsAvailable
								&& r.Identifier.Equals(deviceRuntimeIdentifier, StringComparison.OrdinalIgnoreCase));

							if (runtime != null)
							{
								device.Runtime = runtime;
								deviceType.Devices.Add(device);
							}
						}
					}
				}

				if (deviceType.Devices.Any())
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
		public string BundlePath { get; set; }

		[JsonProperty("buildVersion")]
		public string BuildVersion { get; set; }

		[JsonProperty("runtimeRoot")]
		public string RuntimeRoot { get; set; }

		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("isAvailable")]
		public bool IsAvailable { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public class SimCtlDeviceType
	{
		[JsonProperty("minRuntimeVersion")]
		public long MinRuntimeVersion { get; set; }

		[JsonProperty("bundlePath")]
		public string BundlePath { get; set; }

		[JsonProperty("maxRuntimeVersion")]
		public long MaxRuntimeVersion { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		[JsonProperty("productFamily")]
		public string ProductFamily { get; set; }

		[JsonProperty("devices")]
		public List<SimCtlDevice> Devices { get; set; } = new List<SimCtlDevice>();
	}

	public class SimCtlDevice
	{
		[JsonProperty("dataPath")]
		public string DataPath { get; set; }

		[JsonProperty("logPath")]
		public string LogPath { get; set; }

		[JsonProperty("udid")]
		public string Udid { get; set; }

		[JsonProperty("isAvailable")]
		public bool IsAvailable { get; set; }

		[JsonProperty("deviceTypeIdentifier")]
		public string DeviceTypeIdentifier { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("availabilityError")]
		public string AvailabilityError { get; set; }

		[JsonProperty("deviceType")]
		public SimCtlDeviceType DeviceType { get; set; }

		[JsonProperty("runtime")]
		public SimCtlRuntime Runtime { get; set; }
	}
}
