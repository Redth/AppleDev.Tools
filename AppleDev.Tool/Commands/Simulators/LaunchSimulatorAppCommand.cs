using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class LaunchSimulatorAppCommand : AsyncCommand<LaunchSimulatorAppCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, LaunchSimulatorAppCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			AnsiConsole.MarkupLine($"Launching [cyan]{settings.BundleId}[/] on simulator [cyan]{settings.Target}[/]...");

			var success = await simctl.LaunchAppAsync(settings.Target, settings.BundleId, data.CancellationToken);

			if (success)
			{
				AnsiConsole.MarkupLine($"[green]✓ Successfully launched {settings.BundleId}[/]");
				return this.ExitCode(true);
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] Failed to launch app");
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

public class LaunchSimulatorAppCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'My iPhone 16', 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier (e.g., com.myapp.id)")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
		{
			return ValidationResult.Error("Target simulator is required");
		}

		if (string.IsNullOrWhiteSpace(BundleId))
		{
			return ValidationResult.Error("Bundle ID is required");
		}

		return ValidationResult.Success();
	}
}
