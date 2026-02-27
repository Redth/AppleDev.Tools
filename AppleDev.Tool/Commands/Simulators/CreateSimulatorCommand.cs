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
        
        var udid = await simctl.CreateWithUdidAsync(
            settings.Name, 
            settings.DeviceTypeId, 
            settings.RuntimeId,
            data.CancellationToken).ConfigureAwait(false);
        
        var success = udid is not null;
        
        if (success && settings.Boot)
        {
            var bootSuccess = await simctl.BootAsync(udid!, data.CancellationToken).ConfigureAwait(false);
            
            if (bootSuccess && settings.Wait)
                bootSuccess = await simctl.WaitForBootedAsync(udid!, TimeSpan.FromSeconds(settings.Timeout), data.CancellationToken).ConfigureAwait(false);
            
            if (!bootSuccess)
            {
                AnsiConsole.MarkupLine($"[red]Simulator created but failed to boot '{settings.Name}'[/]");
                
                if (settings.Format == OutputFormat.Json || settings.Format == OutputFormat.JsonPretty)
                {
                    var errorResult = new { udid = udid, name = settings.Name, error = $"Simulator created but failed to boot '{settings.Name}'" };
                    OutputHelper.Output(errorResult, settings.Format);
                }
                
                return this.ExitCode(false);
            }
        }
        
        if (success)
        {
            var format = settings.Format;
            if (format == OutputFormat.None)
            {
                AnsiConsole.MarkupLine($"[green]Successfully created simulator '{settings.Name}'[/]");
                AnsiConsole.WriteLine(udid!);
            }
            else if (format == OutputFormat.Json || format == OutputFormat.JsonPretty)
            {
                var result = new { udid, name = settings.Name, deviceType = settings.DeviceTypeId, runtime = settings.RuntimeId };
                OutputHelper.Output(result, format);
            }
            else if (format == OutputFormat.Xml)
            {
                OutputHelper.Output(udid!, format);
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to create simulator '{settings.Name}'[/]");
        }
        
        return this.ExitCode(success);
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

    [Description("Boot the simulator after creation")]
    [CommandOption("--boot")]
    [DefaultValue(false)]
    public bool Boot { get; set; }

    [Description("Wait for the simulator to be fully ready (requires --boot)")]
    [CommandOption("--wait")]
    [DefaultValue(false)]
    public bool Wait { get; set; }

    [Description("Timeout in seconds to wait for boot readiness")]
    [CommandOption("--timeout")]
    [DefaultValue(120)]
    public int Timeout { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return ValidationResult.Error("Simulator name is required");
        
        if (string.IsNullOrWhiteSpace(DeviceTypeId))
            return ValidationResult.Error("Device type identifier is required");

        if (Wait && !Boot)
            return ValidationResult.Error("--wait requires --boot");

        if (Wait && Timeout <= 0)
            return ValidationResult.Error("--timeout must be > 0");
        
        return base.Validate();
    }
}
