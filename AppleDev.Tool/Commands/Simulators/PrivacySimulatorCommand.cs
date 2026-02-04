using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class GrantPrivacySimulatorCommand : AsyncCommand<GrantPrivacySimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, GrantPrivacySimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Granting [cyan]{settings.Permission}[/] permission to [cyan]{settings.BundleId}[/] on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.GrantPrivacyAsync(settings.Target, settings.Permission, settings.BundleId, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Permission granted successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to grant permission");
			return this.ExitCode(false);
		}
	}
}

public class GrantPrivacySimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Permission to grant (e.g., 'photos', 'camera', 'location', 'microphone', 'contacts')")]
	[CommandArgument(1, "<permission>")]
	public string Permission { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(2, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		if (string.IsNullOrWhiteSpace(Permission))
			return ValidationResult.Error("Permission is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		return ValidationResult.Success();
	}
}

public class RevokePrivacySimulatorCommand : AsyncCommand<RevokePrivacySimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, RevokePrivacySimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Revoking [cyan]{settings.Permission}[/] permission from [cyan]{settings.BundleId}[/] on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.RevokePrivacyAsync(settings.Target, settings.Permission, settings.BundleId, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Permission revoked successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to revoke permission");
			return this.ExitCode(false);
		}
	}
}

public class RevokePrivacySimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Permission to revoke (e.g., 'photos', 'camera', 'location', 'microphone', 'contacts')")]
	[CommandArgument(1, "<permission>")]
	public string Permission { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(2, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		if (string.IsNullOrWhiteSpace(Permission))
			return ValidationResult.Error("Permission is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		return ValidationResult.Success();
	}
}

public class ResetPrivacySimulatorCommand : AsyncCommand<ResetPrivacySimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ResetPrivacySimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		var targetDesc = string.IsNullOrEmpty(settings.BundleId) ? "all apps" : settings.BundleId;
		AnsiConsole.MarkupLine($"Resetting [cyan]{settings.Permission}[/] permission for [cyan]{targetDesc}[/] on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.ResetPrivacyAsync(settings.Target, settings.Permission, settings.BundleId, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Permission reset successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to reset permission");
			return this.ExitCode(false);
		}
	}
}

public class ResetPrivacySimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Permission to reset (e.g., 'photos', 'camera', 'location', 'all')")]
	[CommandArgument(1, "<permission>")]
	public string Permission { get; set; } = string.Empty;

	[Description("Bundle identifier of the app (optional, if omitted resets for all apps)")]
	[CommandArgument(2, "[bundle-id]")]
	public string? BundleId { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		if (string.IsNullOrWhiteSpace(Permission))
			return ValidationResult.Error("Permission is required");

		return ValidationResult.Success();
	}
}
