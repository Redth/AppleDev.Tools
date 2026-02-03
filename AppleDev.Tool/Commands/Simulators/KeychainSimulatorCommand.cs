using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ResetKeychainSimulatorCommand : AsyncCommand<ResetKeychainSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ResetKeychainSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Resetting keychain on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.ResetKeychainAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Keychain reset successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to reset keychain");
			return this.ExitCode(false);
		}
	}
}

public class ResetKeychainSimulatorCommandSettings : CommandSettings
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
