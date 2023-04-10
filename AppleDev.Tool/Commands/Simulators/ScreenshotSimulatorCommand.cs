using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ScreenshotSimulatorCommand : AsyncCommand<ScreenshotSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScreenshotSimulatorCommandSettings settings)
    {
        var data = context.GetData();
        var simctl = new SimCtl();

        var path = settings.Output;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = Path.GetTempFileName() + ".png";
        }
        else
        {
            if (!settings.IsFile)
                path = Path.Combine(path, $"Screenshot-{DateTime.UtcNow.ToLongTimeString()}.png");
        }
        
        var success = await simctl.RecordScreenshotAsync(settings.Udid, new FileInfo(path), data.CancellationToken).ConfigureAwait(false);

        OutputHelper.OutputObject(
            new ScreenshotResultOutput { Path = path },
            new [] {"Path" },
            r => new [] { r.Path });
        
        return this.ExitCode(success);
    }

    class ScreenshotResultOutput
    {
        public string Path { get; set; } = string.Empty;
    }
}
public class ScreenshotSimulatorCommandSettings : FormattableOutputCommandSettings
{
    [CommandArgument(0,"<udid>")]
    public string Udid { get; set; } = string.Empty;
    
    [Description("Output path (file or directory)")]
    
    [CommandOption("-o|--output <PATH>")]
    public string? Output { get; set; }

    internal bool IsFile
        => !string.IsNullOrWhiteSpace(Output) && !Path.EndsInDirectorySeparator(Output);

    public override ValidationResult Validate()
    {
        if (!string.IsNullOrWhiteSpace(Output))
        {
            if (Path.EndsInDirectorySeparator(Output))
            {
                var invalidPathChars = Path.GetInvalidPathChars();
                if (Output.Any(c => invalidPathChars.Contains(c)))
                    return ValidationResult.Error("Invalid path specified for --output");
            }
            else
            {
                var invalidFileChars = Path.GetInvalidFileNameChars();
                if (Output.Any(c => invalidFileChars.Contains(c)))
                    return ValidationResult.Error("Invalid path specified for --output");
            }
        }
        
        return base.Validate();
    }
}