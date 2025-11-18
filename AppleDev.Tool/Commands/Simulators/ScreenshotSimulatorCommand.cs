using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ScreenshotSimulatorCommand : AsyncCommand<ScreenshotSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScreenshotSimulatorCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        var simctl = new SimCtl();

        var path = settings.GetOutputFile("screenshot", ".png");

        var success = await simctl.RecordScreenshotAsync(settings.Target, path, data.CancellationToken).ConfigureAwait(false);

        OutputHelper.OutputObject(
            new ScreenshotResultOutput { Path = path.FullName },
            new [] {"Path" },
            r => new [] { r.Path });
        
        return this.ExitCode(success);
    }

    class ScreenshotResultOutput
    {
        public string Path { get; set; } = string.Empty;
    }
}
public class ScreenshotSimulatorCommandSettings : FormattableOutputCommandSettings, IOutputCommandSettings
{
    [CommandArgument(0,"<target>")]
    public string Target { get; set; } = string.Empty;
    
    [Description("Output path (file or directory)")]
    [CommandOption("-o|--output <PATH>")]
    public string? Output { get; set; }

    public override ValidationResult Validate()
    {
        if (!this.IsValid(out var r))
            return r;

        return base.Validate();
    }
}