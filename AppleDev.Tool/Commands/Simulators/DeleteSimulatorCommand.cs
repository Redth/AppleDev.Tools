using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeleteSimulatorCommand : AsyncCommand<DeleteSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DeleteSimulatorCommandSettings settings)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        
        var success = await simctl.DeleteAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);
        
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

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Target))
            return ValidationResult.Error("Target is required");
        
        return base.Validate();
    }
}
