using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class EraseSimulatorCommand : AsyncCommand<EraseSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EraseSimulatorCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        var success = await simctl.EraseAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

        return this.ExitCode(success);
    }
}
public class EraseSimulatorCommandSettings : CommandSettings
{
    [Description("Target Simulator(s) (eg: UDID, Name, Booted, or all")]
    
    [CommandArgument(0,"<target>")]
    public string Target { get; set; } = string.Empty;
}