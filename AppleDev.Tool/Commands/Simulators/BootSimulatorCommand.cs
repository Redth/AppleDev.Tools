using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class BootSimulatorCommand : AsyncCommand<BootSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BootSimulatorCommandSettings settings)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        var success = await simctl.BootAsync(settings.Udid, data.CancellationToken).ConfigureAwait(false);

        if (success && settings.Wait)
            success = await simctl.WaitForBootedAsync(settings.Udid, TimeSpan.FromSeconds(settings.Timeout), data.CancellationToken).ConfigureAwait(false);
        
        return this.ExitCode(success);
    }
}
public class BootSimulatorCommandSettings : CommandSettings
{
    [Description("Wait Until Ready")]
    [CommandOption("--wait")]
    [DefaultValue(false)]
    public bool Wait { get; set; }
    
    [Description("Timeout to wait until ready")]
    [CommandOption("--timeout")]
    [DefaultValue(120)]
    public uint Timeout { get; set; }

    [Description("Simulator UDID")]
    
    [CommandArgument(0,"<udid>")]
    public string Udid { get; set; } = string.Empty;

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Udid))
            return ValidationResult.Error("--udid is required");
        
        if (Wait && Timeout <= 0)
            return ValidationResult.Error("--timeout must be > 0");
        
        return base.Validate();
    }
}