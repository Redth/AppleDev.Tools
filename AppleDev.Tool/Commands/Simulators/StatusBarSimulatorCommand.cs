using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class SetStatusBarSimulatorCommand : AsyncCommand<SetStatusBarSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, SetStatusBarSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		var options = new StatusBarOptions
		{
			Time = settings.Time,
			DataNetwork = settings.DataNetwork,
			WifiMode = settings.WifiMode,
			WifiBars = settings.WifiBars,
			CellularMode = settings.CellularMode,
			CellularBars = settings.CellularBars,
			OperatorName = settings.OperatorName,
			BatteryState = settings.BatteryState,
			BatteryLevel = settings.BatteryLevel
		};

		AnsiConsole.MarkupLine($"Setting status bar overrides on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.SetStatusBarAsync(settings.Target, options, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Status bar updated successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to update status bar");
			return this.ExitCode(false);
		}
	}
}

public class SetStatusBarSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Time to display (e.g., '9:41')")]
	[CommandOption("--time <TIME>")]
	public string? Time { get; set; }

	[Description("Data network type: wifi, 3g, 4g, lte, lte-a, lte+, 5g, 5g-uwb, 5g+, 5g-uc")]
	[CommandOption("--data-network <TYPE>")]
	public string? DataNetwork { get; set; }

	[Description("WiFi mode: searching, failed, active")]
	[CommandOption("--wifi-mode <MODE>")]
	public string? WifiMode { get; set; }

	[Description("Number of WiFi bars (0-3)")]
	[CommandOption("--wifi-bars <BARS>")]
	public int? WifiBars { get; set; }

	[Description("Cellular mode: notSupported, searching, failed, active")]
	[CommandOption("--cellular-mode <MODE>")]
	public string? CellularMode { get; set; }

	[Description("Number of cellular bars (0-4)")]
	[CommandOption("--cellular-bars <BARS>")]
	public int? CellularBars { get; set; }

	[Description("Carrier/operator name")]
	[CommandOption("--operator <NAME>")]
	public string? OperatorName { get; set; }

	[Description("Battery state: charging, charged, discharging")]
	[CommandOption("--battery-state <STATE>")]
	public string? BatteryState { get; set; }

	[Description("Battery level percentage (0-100)")]
	[CommandOption("--battery-level <LEVEL>")]
	public int? BatteryLevel { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		// At least one option should be specified
		var hasOption = Time != null || DataNetwork != null || WifiMode != null ||
		                WifiBars.HasValue || CellularMode != null || CellularBars.HasValue ||
		                OperatorName != null || BatteryState != null || BatteryLevel.HasValue;

		if (!hasOption)
			return ValidationResult.Error("At least one status bar option must be specified");

		if (WifiBars.HasValue && (WifiBars < 0 || WifiBars > 3))
			return ValidationResult.Error("WiFi bars must be between 0 and 3");

		if (CellularBars.HasValue && (CellularBars < 0 || CellularBars > 4))
			return ValidationResult.Error("Cellular bars must be between 0 and 4");

		if (BatteryLevel.HasValue && (BatteryLevel < 0 || BatteryLevel > 100))
			return ValidationResult.Error("Battery level must be between 0 and 100");

		return ValidationResult.Success();
	}
}

public class ClearStatusBarSimulatorCommand : AsyncCommand<ClearStatusBarSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ClearStatusBarSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Clearing status bar overrides on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.ClearStatusBarAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Status bar cleared successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to clear status bar");
			return this.ExitCode(false);
		}
	}
}

public class ClearStatusBarSimulatorCommandSettings : CommandSettings
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
