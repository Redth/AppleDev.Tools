using System.ComponentModel;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ParseProvisioningProfileCommand : AsyncCommand<ParseProvisioningProfileCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ParseProvisioningProfileCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();

        var fileData = await File.ReadAllBytesAsync(settings.ProvisioningProfileFile!.FullName);
        
        var provProfiles = await ProvisioningProfiles.ParseAsync(fileData);
		
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


public class ParseProvisioningProfileCommandSettings : FormattableOutputAppStoreConnectCommand
{
    [Description("Provisioning Profile file to parse")]
    [CommandArgument(0, "<provisioningprofile_file>")]
    [TypeConverter(typeof(FileInfoTypeConverter))]
    public FileInfo? ProvisioningProfileFile { get; set; }
}