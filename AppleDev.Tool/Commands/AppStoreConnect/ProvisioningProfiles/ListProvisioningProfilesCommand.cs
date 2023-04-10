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

		var profileAttributes = results.Data.Select(d => d.Attributes).ToList();

		OutputHelper.Output(profileAttributes, settings.Format, settings.Verbose,
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
}
