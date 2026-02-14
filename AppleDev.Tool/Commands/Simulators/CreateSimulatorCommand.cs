using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace AppleDev.Tool.Commands;

public class CreateSimulatorCommand : AsyncCommand<CreateSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CreateSimulatorCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        
        // Resolve runtime if provided as a friendly name (e.g., "latest", "26.2", "iOS 26.2")
        var runtimeId = settings.RuntimeId;
        if (!string.IsNullOrEmpty(runtimeId) && !runtimeId.StartsWith("com.apple.CoreSimulator"))
        {
            var resolved = await simctl.ResolveRuntimeAsync(runtimeId, cancellationToken: data.CancellationToken).ConfigureAwait(false);
            if (resolved == null)
            {
                AnsiConsole.MarkupLine($"[red]Could not resolve runtime '{runtimeId}'[/]");
                return this.ExitCode(false);
            }
            AnsiConsole.MarkupLine($"[dim]Resolved runtime '{runtimeId}' → {resolved.Name} ({resolved.Identifier})[/]");
            runtimeId = resolved.Identifier;
        }

        var udid = await simctl.CreateAndGetUdidAsync(
            settings.Name, 
            settings.DeviceTypeId, 
            runtimeId,
            data.CancellationToken).ConfigureAwait(false);
        
        if (udid != null)
        {
            if (settings.Format == OutputFormat.Json)
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { udid, name = settings.Name, runtime = runtimeId }));
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]Successfully created simulator '{settings.Name}' ({udid})[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to create simulator '{settings.Name}'[/]");
        }
        
        return this.ExitCode(udid != null);
    }
}

public class CreateSimulatorCommandSettings : FormattableOutputCommandSettings
{
    [Description("Name for the new simulator")]
    [CommandArgument(0, "<name>")]
    public string Name { get; set; } = string.Empty;

    [Description("Device type identifier (e.g., 'iPhone 15' or 'com.apple.CoreSimulator.SimDeviceType.iPhone-15')")]
    [CommandOption("-d|--device-type")]
    [Required]
    public string DeviceTypeId { get; set; } = string.Empty;

    [Description("Runtime identifier (optional - if not specified, newest compatible runtime is used)")]
    [CommandOption("-r|--runtime")]
    public string? RuntimeId { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return ValidationResult.Error("Simulator name is required");
        
        if (string.IsNullOrWhiteSpace(DeviceTypeId))
            return ValidationResult.Error("Device type identifier is required");
        
        return base.Validate();
    }
}
