using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class OpenUrlSimulatorCommand : AsyncCommand<OpenUrlSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, OpenUrlSimulatorCommandSettings settings)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out var uri))
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] Invalid URL format: {settings.Url}");
				return this.ExitCode(false);
			}

			AnsiConsole.MarkupLine($"Opening [cyan]{settings.Url}[/] on simulator [cyan]{settings.Target}[/]...");

			var success = await simctl.OpenUrlAsync(settings.Target, uri, data.CancellationToken);

			if (success)
			{
				AnsiConsole.MarkupLine($"[green]✓ Successfully opened URL[/]");
				return this.ExitCode();
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] Failed to open URL");
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

public class OpenUrlSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'My iPhone 16', 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("URL to open (e.g., 'myapp://deeplink/action', 'https://example.com')")]
	[CommandArgument(1, "<url>")]
	public string Url { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
		{
			return ValidationResult.Error("Target simulator is required");
		}

		if (string.IsNullOrWhiteSpace(Url))
		{
			return ValidationResult.Error("URL is required");
		}

		return ValidationResult.Success();
	}
}
