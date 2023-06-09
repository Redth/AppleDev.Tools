﻿using System.ComponentModel;
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
		var config = new AppStoreConnectConfiguration(settings.KeyId, settings.IssuerId, settings.GetPrivateKeyBase64());
		
		var profileResults = new List<ProvisioningProfile>();

		var appStoreConnect = new AppStoreConnectClient(config);

		var profiles = await appStoreConnect.ListProfilesAsync(
			filterProfileState: settings.Active ? new[] { ProfileState.ACTIVE } : null,
			filterProfileType: settings.ProfileTypes.Length > 0 ? settings.ProfileTypes : null,
			include: "bundleId",
			limit: 200)
			.ConfigureAwait(false);

		foreach (var profile in profiles.Data)
		{
			// Get the Bundle ID for the profile
			var profileBundleId = profiles.IncludedBundleIds?.FirstOrDefault()?.Attributes;

			if (settings.BundleIds.Length > 0)
			{
				if (profileBundleId is not null && settings.BundleIds.Any(b => profileBundleId?.IdentifierMatches(b) ?? false))
					profileResults.Add(new ProvisioningProfile(profile.Attributes, profileBundleId));
			}
			else
			{
				profileResults.Add(new ProvisioningProfile(profile.Attributes, profileBundleId));
			}
		}

		if (settings.Download)
		{
			await appStoreConnect.InstallProfilesAsync(profileResults.Select(p => p.Profile), settings.DownloadPath).ConfigureAwait(false);
		}

		OutputHelper.Output(profileResults, settings.Format, settings.Verbose,
			new ColumnInfo<ProvisioningProfile>("Name", r => r.Profile.Name),
			new ColumnInfo<ProvisioningProfile>("UUID", r => r.Profile.Uuid),
			new ColumnInfo<ProvisioningProfile>("Profile Type", r => r.Profile.ProfileType.GetEnumMemberValue() ?? ""),
			new ColumnInfo<ProvisioningProfile>("State", r => r.Profile.ProfileState.GetEnumMemberValue() ?? ""),
			new ColumnInfo<ProvisioningProfile>("Platform", r => r.Profile.Platform.GetEnumMemberValue()),
			new ColumnInfo<ProvisioningProfile>("Bundle Identifier", r => r.BundleId?.Identifier));

		return this.ExitCode();
	}
}

public class ProvisioningProfile
{
	public ProvisioningProfile() { }

	public ProvisioningProfile(ProfileAttributes profile, BundleIdAttributes? bundleId)
	{
		Profile = profile;
		BundleId = bundleId;
	}

	[JsonPropertyName("profile")]
	public ProfileAttributes Profile { get; set; } = new ();

	[JsonPropertyName("bundleId")]
	public BundleIdAttributes? BundleId { get; set; }
}

public class ListProvisioningProfilesCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Filter to provisioning profile type")]
	[CommandOption("-t|--type <type>")]
	[DefaultValue(ProfileType.IOS_APP_DEVELOPMENT)]
	[TypeConverter(typeof(StringEnumTypeConverter<ProfileType>))]
	public ProfileType[] ProfileTypes { get; set; } = new ProfileType[0];

	[Description("Only list active")]
	[CommandOption("-a|--active")]
	public bool Active { get; set; } = false;

	[Description("Download and install the profiles locally")]
	[CommandOption("-d|--download")]
	public bool Download { get; set; } = false;

	[Description("Override default directory to save downloaded profiles to")]
	[CommandOption("--download-path <directory>")]
	[TypeConverter(typeof(DirectoryInfoTypeConverter))]
	public DirectoryInfo? DownloadPath { get; set; }

	[Description("Filter Profiles by BundleID")]
	[CommandOption("-b|--bundle-id <bundle-id>")]
	public string[] BundleIds { get; set; } = new string[0];
}
