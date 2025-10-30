using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class OpenSimulatorCommand : AsyncCommand<OpenSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, OpenSimulatorCommandSettings settings)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			if (string.IsNullOrWhiteSpace(settings.Udid))
			{
				AnsiConsole.MarkupLine("Opening Simulator.app...");
			}
			else
			{
				AnsiConsole.MarkupLine($"Opening Simulator.app to [cyan]{settings.Udid}[/]...");
			}

			var success = await simctl.OpenSimulatorAppAsync(
				string.IsNullOrWhiteSpace(settings.Udid) ? null : settings.Udid,
				data.CancellationToken);

			if (success)
			{
				AnsiConsole.MarkupLine("[green]✓ Successfully opened Simulator.app[/]");
				return this.ExitCode();
			}
			else
			{
				AnsiConsole.MarkupLine("[red]Error:[/] Failed to open Simulator.app");
				return this.ExitCode(false);
			}
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class OpenSimulatorCommandSettings : CommandSettings
{
	[Description("Optional simulator UDID to open to a specific simulator")]
	[CommandArgument(0, "[udid]")]
	public string? Udid { get; set; }
}
