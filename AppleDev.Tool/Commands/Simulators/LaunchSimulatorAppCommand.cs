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

			var envVars = settings.ParseEnvironmentVariables();

			var success = await simctl.LaunchAppAsync(settings.Target, settings.BundleId, envVars, data.CancellationToken);

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

	[Description("Environment variable to pass to the launched app (KEY=VALUE). Can be specified multiple times.")]
	[CommandOption("--env|-e")]
	public string[]? EnvironmentVariables { get; set; }

	internal IReadOnlyDictionary<string, string>? ParseEnvironmentVariables()
	{
		if (EnvironmentVariables is null || EnvironmentVariables.Length == 0)
			return null;

		var dict = new Dictionary<string, string>();
		foreach (var env in EnvironmentVariables)
		{
			var eqIndex = env.IndexOf('=');
			if (eqIndex <= 0)
			{
				throw new InvalidOperationException($"Invalid environment variable format: '{env}'. Expected KEY=VALUE.");
			}
			dict[env[..eqIndex]] = env[(eqIndex + 1)..];
		}
		return dict;
	}

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

		try
		{
			ParseEnvironmentVariables();
		}
		catch (InvalidOperationException ex)
		{
			return ValidationResult.Error(ex.Message);
		}

		return ValidationResult.Success();
	}
}
