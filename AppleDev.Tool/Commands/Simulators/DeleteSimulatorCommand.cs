using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeleteSimulatorCommand : AsyncCommand<DeleteSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeleteSimulatorCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        var simctl = new SimCtl();

        if (settings.Force)
        {
            // Shutdown first, ignoring failures (simulator may not be running)
            await simctl.ShutdownAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);
        }
        
        var success = await simctl.DeleteAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

        if (settings.Force)
        {
            // --force never fails (cleanup command)
            if (!success)
                AnsiConsole.MarkupLine($"[yellow]Simulator '{settings.Target}' may not exist (ignored with --force)[/]");
            return this.ExitCode(true);
        }
        
        if (success)
        {
            AnsiConsole.MarkupLine($"[green]Successfully deleted simulator(s): '{settings.Target}'[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to delete simulator(s): '{settings.Target}'[/]");
        }

        return this.ExitCode(success);
    }
}

public class DeleteSimulatorCommandSettings : CommandSettings
{
    [Description("Target simulator(s) to delete (UDID, Name, unavailable, or all)")]
    [CommandArgument(0, "<target>")]
    public string Target { get; set; } = string.Empty;

    [Description("Shutdown the simulator before deleting (never fails)")]
    [CommandOption("--force")]
    [DefaultValue(false)]
    public bool Force { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Target))
            return ValidationResult.Error("Target is required");
        
        return base.Validate();
    }
}
