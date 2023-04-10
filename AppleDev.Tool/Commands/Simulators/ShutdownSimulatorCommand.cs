using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ShutdownSimulatorCommand : AsyncCommand<ShutdownSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ShutdownSimulatorCommandSettings settings)
    {
        var data = context.GetData();
        
        var simctl = new SimCtl();
        var success = await simctl.ShutdownAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

        return this.ExitCode(success);
    }
}
public class ShutdownSimulatorCommandSettings : CommandSettings
{
    
    [Description("Target Simulator(s) (eg: UDID, Name, Booted, or all")]
    
    [CommandArgument(0,"<target>")]
    public string Target { get; set; } = string.Empty;
}