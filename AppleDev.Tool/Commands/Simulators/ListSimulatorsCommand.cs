using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlDevice>;
using ValidationResult = Spectre.Console.ValidationResult;

namespace AppleDev.Tool.Commands;

public class ListSimulatorsCommand : AsyncCommand<ListSimulatorsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ListSimulatorsCommandSettings settings)
    {
        var data = context.GetData();
        
        var s = new SimCtl();
        var sims = await s.GetSimulatorsAsync(cancellationToken: data.CancellationToken).ConfigureAwait(false);

        if (settings.Available)
            sims = sims.Where(s => s.IsAvailable).ToList();
        if (settings.Unavailable)
            sims = sims.Where(s => !s.IsAvailable).ToList();
        if (settings.Booted)
            sims = sims.Where(s => s.IsBooted).ToList();

        OutputHelper.Output(sims, settings.Format, settings.Verbose,
            new Col("Name", d => d.Name),
            new Col("UDID", d => d.Udid),
            new Col("State", d => d.State),
            new Col("Device Type", d => d.DeviceType?.Name),
            new Col("Runtime", d => d.Runtime?.Name),
            new Col("Runtime Version", d => d.Runtime?.Version, true),
            new Col("Data Path", d => d.DataPath, true),
            new Col("Log Path", d => d.LogPath, true));
    
        return this.ExitCode();
    }
}

public class ListSimulatorsCommandSettings : FormattableOutputCommandSettings
{
    [Description("Booted")]
    [CommandOption("--booted")]
    [DefaultValue(false)]
    public bool Booted { get; set; }
    
    [Description("Available")]
    [CommandOption("--available")]
    [DefaultValue(false)]
    public bool Available { get; set; }
    
    [Description("Unavailable")]
    [CommandOption("--unavailable")]
    [DefaultValue(false)]
    public bool Unavailable { get; set; }

    public override ValidationResult Validate()
    {
        if (Available && Unavailable)
            return ValidationResult.Error("Specify either --available or --unavailable, not both.");
        return base.Validate();
    }
}



public class BaseSimulatorCommandSettings : CommandSettings
{
    [Description("Target")]
    [CommandOption("-t|--target <TARGET>")]
    public string? Target { get; set; }
}