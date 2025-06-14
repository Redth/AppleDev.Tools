using System.ComponentModel;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlApp>;
using ValidationResult = Spectre.Console.ValidationResult;

namespace AppleDev.Tool.Commands;

public class ListAppsCommand : AsyncCommand<ListAppsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ListAppsCommandSettings settings)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        
        var apps = await simctl.GetAppsAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

        OutputHelper.Output(apps, settings.Format, settings.Verbose,
            new Col("Bundle ID", app => app.CFBundleIdentifier ?? "N/A"),
            new Col("App Name", app => app.CFBundleDisplayName ?? app.CFBundleName ?? "N/A"),
            new Col("App Type", app => app.ApplicationType ?? "N/A"),
            new Col("CFBundleVersion", app => app.CFBundleVersion ?? "N/A", true),
            new Col("Path", app => app.Path ?? "N/A", true));
    
        return this.ExitCode();
    }
}

public class ListAppsCommandSettings : FormattableOutputCommandSettings
{
    [Description("Target simulator to list apps from (UDID, Name, or 'booted' for all booted simulators)")]
    [CommandArgument(0, "<target>")]
    public string Target { get; set; } = "booted";

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Target))
            return ValidationResult.Error("Target is required");
        
        return base.Validate();
    }
}
