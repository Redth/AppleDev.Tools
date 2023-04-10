using System.ComponentModel;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class FormattableOutputCommandSettings : CommandSettings
{
    [Description("Output Format")]
    [CommandOption("-f|--format")]
    [DefaultValue(OutputFormat.None)]
    [TypeConverter(typeof(OutputFormatTypeConverter))]
    public OutputFormat Format { get; set; }
    
    [Description("Verbose")]
    [CommandOption("-v|--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; set; }
}

public class ListSimulatorsCommand : AsyncCommand<ListSimulatorsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ListSimulatorsCommandSettings settings)
    {
        try
        {
            var s = new SimCtl();
            var sims = await s.GetSimulatorsAsync().ConfigureAwait(false);

            OutputHelper.Output(sims, settings?.Format, settings?.Verbose ?? false,
                new[] { "Name", "UDID", "State", "Device Type", "Runtime", "Runtime Version" },
                i => new[] { i.Name, i.Udid, i.State, i.DeviceType?.Name, i.Runtime?.Name, i.Runtime?.Version },
                new[] { "Data Path", "Log Path" },
                j => new[] { j.DataPath, j.LogPath });
        }
        catch (Exception ex)
        {
            ex.Output();
            return 1;
        }
        return 0;
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
}