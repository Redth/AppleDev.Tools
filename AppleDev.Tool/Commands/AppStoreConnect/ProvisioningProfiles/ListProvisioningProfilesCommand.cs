using System.ComponentModel;
using System.Text.Json.Serialization;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListProvisioningProfilesCommand : AsyncCommand<ListProvisioningProfilesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListProvisioningProfilesCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());

		var profileResults = new List<ProvisioningProfile>();

		var appStoreConnect = new AppStoreConnectClient(config);

		var profiles = await appStoreConnect.ListProfilesAsync(
			filterProfileState: settings.Active ? new[] { ProfileState.ACTIVE } : null,
			filterProfileType: settings.ProfileTypes.Length > 0 ? settings.ProfileTypes : null,
			include: "bundleId,devices",
			limit: 200)
			.ConfigureAwait(false);

		// Debug: Check what we got back
		if (Environment.GetEnvironmentVariable("APPLEDEV_DEBUG") == "1")
		{
			AnsiConsole.MarkupLine($"[dim]Total profiles: {profiles.Data?.Count() ?? 0}[/]");
			AnsiConsole.MarkupLine($"[dim]Included bundle IDs: {profiles.IncludedBundleIds?.Count() ?? 0}[/]");
		}

		// Collect all unique device IDs from all profiles
		var allDeviceIds = new HashSet<string>();
		foreach (var profile in profiles.Data)
		{
			if (profile.Relationships.TryGetValue("devices", out var devRel) && devRel?.Data != null)
			{
				foreach (var device in devRel.Data)
				{
					allDeviceIds.Add(device.Id);
				}
			}
		}

		// Fetch all devices in one call if there are any
		Dictionary<string, DeviceAttributes> deviceLookup = new();
		if (allDeviceIds.Count > 0)
		{
			if (Environment.GetEnvironmentVariable("APPLEDEV_DEBUG") == "1")
			{
				AnsiConsole.MarkupLine($"[dim]Fetching {allDeviceIds.Count} unique devices...[/]");
			}

			var deviceResponse = await appStoreConnect.ListDevicesAsync(
				filterId: allDeviceIds.ToArray(),
				limit: 200)
				.ConfigureAwait(false);

			if (Environment.GetEnvironmentVariable("APPLEDEV_DEBUG") == "1")
			{
				AnsiConsole.MarkupLine($"[dim]Received {deviceResponse.Data?.Count() ?? 0} devices[/]");
			}

			foreach (var device in deviceResponse.Data)
			{
				deviceLookup[device.Id] = device.Attributes;
			}
		}

		foreach (var profile in profiles.Data)
		{
			// Get the Bundle ID for this specific profile via relationships
			string? bundleIdId = profile.Relationships.TryGetValue("bundleId", out var bundleIdRel) && bundleIdRel.Data?.Count > 0
				? bundleIdRel.Data[0].Id
				: null;

			var profileBundleId = bundleIdId != null
				? profiles.IncludedBundleIds?.FirstOrDefault(b => b.Id == bundleIdId)?.Attributes
				: null;

			// Get devices for this profile via relationships
			var deviceIds = new List<string>();
			if (profile.Relationships.TryGetValue("devices", out var devRel) && devRel?.Data != null)
			{
				deviceIds = devRel.Data.Select(d => d.Id).ToList();
			}

			// Debug output per profile
			if (Environment.GetEnvironmentVariable("APPLEDEV_DEBUG") == "1")
			{
				if (deviceIds.Count > 0)
					AnsiConsole.MarkupLine($"[dim]Profile '{profile.Attributes.Name}' has {deviceIds.Count} device IDs in relationships[/]");
				else
					AnsiConsole.MarkupLine($"[dim]Profile '{profile.Attributes.Name}' has NO device relationships[/]");
			}

			var profileDevices = new List<DeviceAttributes>();
			if (deviceIds.Count > 0)
			{
				foreach (var deviceId in deviceIds)
				{
					if (deviceLookup.TryGetValue(deviceId, out var deviceAttrs))
					{
						profileDevices.Add(deviceAttrs);
					}
				}
			}

			// Debug: Check if we matched any devices
			if (Environment.GetEnvironmentVariable("APPLEDEV_DEBUG") == "1" && deviceIds.Count > 0)
			{
				AnsiConsole.MarkupLine($"[dim]Matched {profileDevices.Count} devices from lookup (of {deviceLookup.Count} total)[/]");
			}

			// Apply bundle ID filter if specified
			if (settings.BundleIds.Length > 0)
			{
				if (profileBundleId is not null && settings.BundleIds.Any(b => profileBundleId?.IdentifierMatches(b) ?? false))
					profileResults.Add(new ProvisioningProfile(profile.Attributes, profileBundleId, profileDevices, settings.Download));
			}
			else
			{
				profileResults.Add(new ProvisioningProfile(profile.Attributes, profileBundleId, profileDevices, settings.Download));
			}
		}

		if (settings.Install)
		{
			foreach (var p in profileResults)
			{
				var profile = p.Profile;
				var profileData = Convert.FromBase64String(profile.ProfileContent);
				await ProvisioningProfiles.InstallProfileAsync(profileData, settings.DownloadPath).ConfigureAwait(false);
			}
		}

		OutputHelper.Output(profileResults, settings.Format, settings.Verbose,
			new ColumnInfo<ProvisioningProfile>("Name", r => r.Profile.Name),
			new ColumnInfo<ProvisioningProfile>("UUID", r => r.Profile.Uuid),
			new ColumnInfo<ProvisioningProfile>("Profile Type", r => r.Profile.ProfileType.GetEnumMemberValue() ?? ""),
			new ColumnInfo<ProvisioningProfile>("State", r => r.Profile.ProfileState.GetEnumMemberValue() ?? ""),
			new ColumnInfo<ProvisioningProfile>("Platform", r => r.Profile.Platform.GetEnumMemberValue()),
			new ColumnInfo<ProvisioningProfile>("Bundle Identifier", r => r.BundleId?.Identifier),
			new ColumnInfo<ProvisioningProfile>("Device Count", r => r.DeviceCount.ToString(), true),
			new ColumnInfo<ProvisioningProfile>("Device UDIDs", r => r.DeviceUdids, true));

		return this.ExitCode();
	}
}

public class ProvisioningProfile
{
	public ProvisioningProfile() { }

	public ProvisioningProfile(ProfileAttributes profile, BundleIdAttributes? bundleId, List<DeviceAttributes>? devices = null, bool includeProfileData = false)
	{
		Profile = profile;
		BundleId = bundleId;
		Devices = devices ?? new List<DeviceAttributes>();

		// Only include the profile content (base64 data) if requested (typically with --download flag)
		if (includeProfileData)
		{
			ProfileData = profile.ProfileContent;
		}
	}

	[JsonPropertyName("profile")]
	public ProfileAttributes Profile { get; set; } = new ();

	[JsonPropertyName("bundleId")]
	public BundleIdAttributes? BundleId { get; set; }

	[JsonPropertyName("devices")]
	public List<DeviceAttributes> Devices { get; set; } = new();

	[JsonPropertyName("profileData")]
	public string? ProfileData { get; set; }

	[JsonIgnore]
	public string DeviceUdids => string.Join(", ", Devices.Select(d => d.Udid));

	[JsonIgnore]
	public int DeviceCount => Devices.Count;
}

public class ListProvisioningProfilesCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Filter to provisioning profile type (e.g., IOS_APP_DEVELOPMENT, IOS_APP_STORE)")]
	[CommandOption("-t|--type <type>")]
	[TypeConverter(typeof(StringEnumTypeConverter<ProfileType>))]
	public ProfileType[] ProfileTypes { get; set; } = Array.Empty<ProfileType>();

	[Description("Only list active")]
	[CommandOption("-a|--active")]
	public bool Active { get; set; } = false;

	[Description("Include profile data (base64) in JSON output")]
	[CommandOption("-d|--download")]
	public bool Download { get; set; } = false;

	[Description("Install the profiles locally")]
	[CommandOption("--install")]
	public bool Install { get; set; } = false;

	[Description("Override default directory to save installed profiles to")]
	[CommandOption("--download-path <directory>")]
	[TypeConverter(typeof(DirectoryInfoTypeConverter))]
	public DirectoryInfo? DownloadPath { get; set; }

	[Description("Filter Profiles by BundleID")]
	[CommandOption("-b|--bundle-id <bundle-id>")]
	public string[] BundleIds { get; set; } = Array.Empty<string>();
}
