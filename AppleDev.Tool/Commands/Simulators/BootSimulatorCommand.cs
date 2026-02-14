using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class BootSimulatorCommand : AsyncCommand<BootSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BootSimulatorCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        var success = await simctl.BootAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

        if (success)
        {
            AnsiConsole.MarkupLine($"[green]Booted simulator '{settings.Target}'[/]");
            
            if (settings.Wait)
            {
                AnsiConsole.MarkupLine($"[dim]Waiting for simulator to be ready (timeout: {settings.Timeout}s)...[/]");
                success = await simctl.WaitForBootedAsync(settings.Target, TimeSpan.FromSeconds(settings.Timeout), data.CancellationToken).ConfigureAwait(false);
                
                if (success)
                    AnsiConsole.MarkupLine($"[green]Simulator '{settings.Target}' is ready[/]");
                else
                    AnsiConsole.MarkupLine($"[red]Simulator '{settings.Target}' failed to become ready within {settings.Timeout}s[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to boot simulator '{settings.Target}'[/]");
        }
        
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
    public int Timeout { get; set; }

    [Description("Target simulator(s) to boot (UDID or Name)")]
    
    [CommandArgument(0,"<target>")]
    public string Target { get; set; } = string.Empty;

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Target))
            return ValidationResult.Error("Target is required");

        if (Wait && Timeout <= 0)
            return ValidationResult.Error("--timeout must be > 0");
        
        return base.Validate();
    }
}