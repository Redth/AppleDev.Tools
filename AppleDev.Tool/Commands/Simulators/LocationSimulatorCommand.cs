using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class SetLocationSimulatorCommand : AsyncCommand<SetLocationSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, SetLocationSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Setting location to [cyan]{settings.Latitude}, {settings.Longitude}[/] on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.SetLocationAsync(settings.Target, settings.Latitude, settings.Longitude, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Location set successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to set location");
			return this.ExitCode(false);
		}
	}
}

public class SetLocationSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Latitude coordinate (e.g., 37.7749)")]
	[CommandArgument(1, "<latitude>")]
	public double Latitude { get; set; }

	[Description("Longitude coordinate (e.g., -122.4194)")]
	[CommandArgument(2, "<longitude>")]
	public double Longitude { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		if (Latitude < -90 || Latitude > 90)
			return ValidationResult.Error("Latitude must be between -90 and 90");

		if (Longitude < -180 || Longitude > 180)
			return ValidationResult.Error("Longitude must be between -180 and 180");

		return ValidationResult.Success();
	}
}

public class ClearLocationSimulatorCommand : AsyncCommand<ClearLocationSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ClearLocationSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Clearing simulated location on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.ClearLocationAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Location cleared successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to clear location");
			return this.ExitCode(false);
		}
	}
}

public class ClearLocationSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		return ValidationResult.Success();
	}
}
