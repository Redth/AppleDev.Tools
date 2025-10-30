using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class UninstallSimulatorAppCommand : AsyncCommand<UninstallSimulatorAppCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, UninstallSimulatorAppCommandSettings settings)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			AnsiConsole.MarkupLine($"Uninstalling [cyan]{settings.BundleId}[/] from simulator [cyan]{settings.Target}[/]...");

			var success = await simctl.UninstallAppAsync(settings.Target, settings.BundleId, data.CancellationToken);

			if (success)
			{
				AnsiConsole.MarkupLine($"[green]✓ Successfully uninstalled {settings.BundleId}[/]");
				return this.ExitCode(true);
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] Failed to uninstall app");
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

public class UninstallSimulatorAppCommandSettings : CommandSettings
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
