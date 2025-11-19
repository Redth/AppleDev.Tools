using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlDevice>;

namespace AppleDev.Tool.Commands;

public class ListSimulatorsCommand : AsyncCommand<ListSimulatorsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ListSimulatorsCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        
        var simctl = new SimCtl();
        var sims = await simctl.GetSimulatorsAsync(availableOnly: false, cancellationToken: data.CancellationToken).ConfigureAwait(false);

        if (settings.Available)
            sims = sims.Where(sim => sim.IsAvailable).ToList();
        if (settings.Unavailable)
            sims = sims.Where(sim => !sim.IsAvailable).ToList();
        if (settings.Booted)
            sims = sims.Where(sim => sim.IsBooted).ToList();
        if (!string.IsNullOrEmpty(settings.Name))
            sims = sims.Where(sim => sim.Name?.Equals(settings.Name, StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (!string.IsNullOrEmpty(settings.Udid))
            sims = sims.Where(sim => sim.Udid?.Equals(settings.Udid, StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (!string.IsNullOrEmpty(settings.Runtime))
            sims = sims.Where(sim => sim.Runtime?.Name?.Equals(settings.Runtime, StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (!string.IsNullOrEmpty(settings.DeviceType))
            sims = sims.Where(sim => sim.DeviceType?.Name?.Equals(settings.DeviceType, StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (!string.IsNullOrEmpty(settings.ProductFamily))
            sims = sims.Where(sim => sim.DeviceType?.ProductFamily?.Equals(settings.ProductFamily, StringComparison.OrdinalIgnoreCase) == true).ToList();

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
    [Description("Show only simulators that are currently booted and running")]
    [CommandOption("--booted")]
    [DefaultValue(false)]
    public bool Booted { get; set; }
    
    [Description("Show only simulators that are available for use")]
    [CommandOption("--available")]
    [DefaultValue(false)]
    public bool Available { get; set; }
    
    [Description("Show only simulators that are unavailable (e.g., unsupported runtime)")]
    [CommandOption("--unavailable")]
    [DefaultValue(false)]
    public bool Unavailable { get; set; }

    [Description("Show only simulators with the specified name")]
    [CommandOption("--name")]
    public string? Name { get; set; }

    [Description("Show only simulators with the specified UDID")]
    [CommandOption("--udid")]
    public string? Udid { get; set; }

    [Description("Show only simulators with the specified runtime (e.g., 'iOS 18.3' or 'tvOS 18.2')")]
    [CommandOption("--runtime")]
    public string? Runtime { get; set; }

    [Description("Show only simulators with the specified device type (e.g., 'iPhone 16 Pro' or 'Apple TV 4K (3rd generation) (at 1080p)')")]
    [CommandOption("--device-type")]
    public string? DeviceType { get; set; }

    [Description("Show only simulators with the specified product family (e.g., 'iPhone' or 'Apple TV')")]
    [CommandOption("--product-family")]
    public string? ProductFamily { get; set; }

    public override ValidationResult Validate()
    {
        if (Available && Unavailable)
            return ValidationResult.Error("Specify either --available or --unavailable, not both.");
        return base.Validate();
    }
}
