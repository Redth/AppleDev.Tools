using System.ComponentModel;
using AppStoreConnect.Model;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListProvisioningProfilesCommand : AsyncCommand<ListProvisioningProfilesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListProvisioningProfilesCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnect.Client.AppStoreConnectConfiguration(settings.KeyId, settings.IssuerId, settings.GetPrivateKeyBase64());
		var api = new AppStoreConnect.Api.ProfilesApi(config);

		var results = await api.ProfilesGetCollectionAsync(
			filterProfileType: settings.ProfileTypes.Any() ? 
				settings.ProfileTypes.Select(t => t.GetEnumMemberValue() ?? "").ToList() : null,
			filterProfileState: settings.Active ? new List<string> { "ACTIVE" } : null,	
			cancellationToken: data.CancellationToken).ConfigureAwait(false);

		var profileResults = new List<ProfileAttributes>();

		// Filter by bundle id if specified
		if (!string.IsNullOrEmpty(settings.BundleId))
		{
			foreach (var profile in results.Data)
			{
				try
				{
					var bapi = new AppStoreConnect.Api.BundleIdsApi(config);
					var bun = await bapi.BundleIdsGetInstanceAsync("");

					var bundleId = await api.ProfilesBundleIdGetToOneRelatedAsync(profile.Id, cancellationToken: data.CancellationToken).ConfigureAwait(false);
					if (settings.BundleId.Equals(bundleId?.Data?.Attributes?.Identifier, StringComparison.OrdinalIgnoreCase))
					{
						profileResults.Add(profile.Attributes);
					}
				}
				catch (Exception ex)
				{
					AnsiConsole.WriteException(ex);
				}
			}
		}
		else
		{
			profileResults.AddRange(results.Data.Select(d => d.Attributes));
		}

		OutputHelper.Output(profileResults, settings.Format, settings.Verbose,
			new ColumnInfo<ProfileAttributes>("Name", r => r.Name),
			new ColumnInfo<ProfileAttributes>("UUID", r => r.Uuid),
			new ColumnInfo<ProfileAttributes>("Profile Type", r => r.ProfileType.GetEnumMemberValue() ?? ""),
			new ColumnInfo<ProfileAttributes>("State", r => r.ProfileState.GetEnumMemberValue() ?? ""),
			new ColumnInfo<ProfileAttributes>("Platform", r => r.Platform.GetEnumMemberValue()));

		return this.ExitCode();
	}
}
public class ListProvisioningProfilesCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Filter to provisioning profile type")]
	[CommandOption("-t|--type <type>")]
	[TypeConverter(typeof(EnumMemberTypeConverter<ProfileAttributes.ProfileTypeEnum>))]
	public ProfileAttributes.ProfileTypeEnum[] ProfileTypes { get; set; } = new ProfileAttributes.ProfileTypeEnum[0];

	[Description("Only list active")]
	[CommandOption("-a|--active")]
	public bool Active { get; set; } = false;

	[Description("Filter Profiles by BundleID")]
	[CommandOption("-b|--bundle-id <bundle-id>")]
	public string? BundleId { get; set; }
}
