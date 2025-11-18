using System.ComponentModel;
using System.Globalization;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlLogEntry>;

namespace AppleDev.Tool.Commands;

public class LogsSimulatorCommand : AsyncCommand<LogsSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, LogsSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			DateTimeOffset? startTime = null;
			if (!string.IsNullOrWhiteSpace(settings.Start))
			{
				if (!DateTimeOffset.TryParse(settings.Start, out var parsedStart))
				{
					AnsiConsole.MarkupLine($"[red]Error:[/] Invalid timestamp format: {settings.Start}");
					AnsiConsole.MarkupLine("[yellow]Expected format:[/] yyyy-MM-dd HH:mm:ss");
					return this.ExitCode(false);
				}
				startTime = parsedStart;
			}

			var logs = await simctl.GetLogsAsync(
				settings.Target,
				settings.Predicate,
				startTime,
				data.CancellationToken);

			if (logs == null || !logs.Any())
			{
				AnsiConsole.MarkupLine($"[yellow]No logs found for simulator {settings.Target}[/]");
				return this.ExitCode();
			}

			OutputHelper.Output(logs, settings.Format, settings.Verbose,
				new Col("Timestamp", d => d.Timestamp),
				new Col("Message Type", d => d.MessageType),
				new Col("Subsystem", d => d.Subsystem),
				new Col("Category", d => d.Category),
				new Col("Event Message", d => d.EventMessage),
				new Col("Process Image Path", d => d.ProcessImagePath, true),
				new Col("Thread ID", d => d.ThreadID.ToString(), true),
				new Col("Event Type", d => d.EventType, true));

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class LogsSimulatorCommandSettings : FormattableOutputCommandSettings
{
	[Description("Simulator UDID or name (e.g., 'My iPhone 16', 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("NSPredicate filter for logs (e.g., 'eventMessage contains \"error\"')")]
	[CommandOption("--predicate")]
	public string? Predicate { get; set; }

	[Description("Start timestamp for logs (e.g., '2025-10-30 10:00:00')")]
	[CommandOption("--start")]
	public string? Start { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
		{
			return ValidationResult.Error("Target simulator is required");
		}

		return ValidationResult.Success();
	}
}
