using System.ComponentModel;
using System.Text.Json.Serialization;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListInstalledProvisioningProfilesCommand : AsyncCommand<ListInstalledProvisioningProfilesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListInstalledProvisioningProfilesCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();

		var provProfiles = await ProvisioningProfiles.ListProfilesAsync();
		
		OutputHelper.Output(
			provProfiles, settings.Format, settings.Verbose,
			new ColumnInfo<ProvisioningProfileInfo>("Name", r => r.Name),
			new ColumnInfo<ProvisioningProfileInfo>("TeamName", r => r.TeamName),
			new ColumnInfo<ProvisioningProfileInfo>("AppIDName", r => r.AppIdName),
			new ColumnInfo<ProvisioningProfileInfo>("CreationDate", r => r.CreationDate.ToUnixTimeSeconds().ToString()),
			new ColumnInfo<ProvisioningProfileInfo>("ExpirationDate", r => r.ExpirationDate.ToUnixTimeSeconds().ToString()),
			new ColumnInfo<ProvisioningProfileInfo>("Platforms", r => "[" + string.Join(", ", r.Platform) + "]"),
			new ColumnInfo<ProvisioningProfileInfo>("UUID", r => r.Uuid));

		return this.ExitCode();
	}
}


public class ListInstalledProvisioningProfilesCommandSettings : FormattableOutputAppStoreConnectCommand
{

}
