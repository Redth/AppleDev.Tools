using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class InstallSimulatorAppCommand : AsyncCommand<InstallSimulatorAppCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, InstallSimulatorAppCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			var appPath = new DirectoryInfo(settings.AppPath);
			if (!appPath.Exists)
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] App bundle not found at '{settings.AppPath}'");
				return this.ExitCode(false);
			}

			AnsiConsole.MarkupLine($"Installing [cyan]{appPath.Name}[/] to simulator [cyan]{settings.Target}[/]...");

			var success = await simctl.InstallAppAsync(settings.Target, appPath, data.CancellationToken);

			if (success)
			{
				AnsiConsole.MarkupLine($"[green]✓ Successfully installed {appPath.Name}[/]");
				return this.ExitCode(true);
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] Failed to install app");
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

public class InstallSimulatorAppCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'My iPhone 16', 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Path to .app bundle")]
	[CommandArgument(1, "<app-path>")]
	public string AppPath { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
		{
			return ValidationResult.Error("Target simulator is required");
		}

		if (string.IsNullOrWhiteSpace(AppPath))
		{
			return ValidationResult.Error("App path is required");
		}

		return ValidationResult.Success();
	}
}
