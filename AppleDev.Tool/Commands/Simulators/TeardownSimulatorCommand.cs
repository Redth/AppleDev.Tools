using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class TeardownSimulatorCommand : AsyncCommand<TeardownSimulatorCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, TeardownSimulatorCommandSettings settings, CancellationToken cancellationToken)
    {
        var data = context.GetData();
        var simctl = new SimCtl();
        var outputDir = settings.OutputDirectory;
        Directory.CreateDirectory(outputDir);

        if (!settings.SkipLogs)
        {
            // Collect log archive
            try
            {
                AnsiConsole.MarkupLine("[dim]Collecting simulator logs...[/]");
                var logArchivePath = Path.Combine(outputDir, "simulator.logarchive");
                var (logSuccess, _) = await RunTimedCommand(
                    "xcrun", $"simctl spawn {settings.Target} log collect --output {logArchivePath}",
                    TimeSpan.FromSeconds(60), data.CancellationToken);
                
                if (logSuccess && File.Exists(logArchivePath))
                    AnsiConsole.MarkupLine($"[green]Log archive saved to {logArchivePath}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Failed to collect log archive: {ex.Message}[/]");
            }

            // Copy CoreSimulator system log
            try
            {
                var coreSimLog = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Logs", "CoreSimulator", settings.Target, "system.log");
                if (File.Exists(coreSimLog))
                {
                    var destPath = Path.Combine(outputDir, "coresimulator-system.log");
                    File.Copy(coreSimLog, destPath, overwrite: true);
                    AnsiConsole.MarkupLine($"[green]CoreSimulator log saved to {destPath}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Failed to copy CoreSimulator log: {ex.Message}[/]");
            }

            // Copy crash reports
            try
            {
                var crashDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Logs", "DiagnosticReports");
                if (Directory.Exists(crashDir))
                {
                    var crashFiles = Directory.GetFiles(crashDir, "*.crash")
                        .Concat(Directory.GetFiles(crashDir, "*.ips"))
                        .Where(f => File.GetLastWriteTimeUtc(f) > DateTime.UtcNow.AddHours(-1))
                        .ToArray();
                    
                    if (crashFiles.Length > 0)
                    {
                        var destCrashDir = Path.Combine(outputDir, "crash-reports");
                        Directory.CreateDirectory(destCrashDir);
                        foreach (var crashFile in crashFiles)
                        {
                            File.Copy(crashFile, Path.Combine(destCrashDir, Path.GetFileName(crashFile)), overwrite: true);
                        }
                        AnsiConsole.MarkupLine($"[green]{crashFiles.Length} crash report(s) saved[/]");
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Failed to collect crash reports: {ex.Message}[/]");
            }
        }

        // Shutdown simulator
        try
        {
            AnsiConsole.MarkupLine("[dim]Shutting down simulator...[/]");
            await simctl.ShutdownAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine("[green]Simulator shutdown complete[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Failed to shutdown: {ex.Message}[/]");
        }

        // Delete simulator if requested
        if (settings.Delete)
        {
            try
            {
                AnsiConsole.MarkupLine("[dim]Deleting simulator...[/]");
                await simctl.DeleteAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);
                AnsiConsole.MarkupLine("[green]Simulator deleted[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Failed to delete: {ex.Message}[/]");
            }
        }

        return this.ExitCode();
    }

    static async Task<(bool Success, string Output)> RunTimedCommand(string command, string args, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            var result = await CliWrap.Cli.Wrap(command)
                .WithArguments(args)
                .WithValidation(CliWrap.CommandResultValidation.None)
                .ExecuteAsync(cts.Token)
                .ConfigureAwait(false);
            return (result.ExitCode == 0, string.Empty);
        }
        catch (OperationCanceledException)
        {
            return (false, "Timed out");
        }
    }
}

public class TeardownSimulatorCommandSettings : CommandSettings
{
    [Description("Target simulator (UDID or Name)")]
    [CommandArgument(0, "<target>")]
    public string Target { get; set; } = string.Empty;

    [Description("Directory to save collected logs and diagnostics")]
    [CommandOption("-o|--output|--output-directory")]
    [DefaultValue("artifacts/logs")]
    public string OutputDirectory { get; set; } = "artifacts/logs";

    [Description("Skip log collection and only shutdown/delete")]
    [CommandOption("--skip-logs")]
    [DefaultValue(false)]
    public bool SkipLogs { get; set; }

    [Description("Delete the simulator after shutdown")]
    [CommandOption("--delete")]
    [DefaultValue(true)]
    public bool Delete { get; set; } = true;

    public override Spectre.Console.ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Target))
            return Spectre.Console.ValidationResult.Error("Target is required");
        return base.Validate();
    }
}
