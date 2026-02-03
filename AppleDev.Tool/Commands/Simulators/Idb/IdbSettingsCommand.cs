using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbSettingsListCommand : AsyncCommand<IdbSettingsListCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbSettingsListCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		try
		{
			var settingsList = await client.ListSettingsAsync(data.CancellationToken).ConfigureAwait(false);

			if (settingsList.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]No settings available[/]");
			}
			else
			{
				foreach (var setting in settingsList)
				{
					AnsiConsole.WriteLine(setting);
				}
			}

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbSettingsListCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		return ValidationResult.Success();
	}
}

public class IdbSettingsGetCommand : AsyncCommand<IdbSettingsGetCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbSettingsGetCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		try
		{
			var value = await client.GetSettingAsync(settings.Setting, settings.Domain, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.WriteLine(value ?? "");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbSettingsGetCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Setting name")]
	[CommandArgument(1, "<setting>")]
	public string Setting { get; set; } = string.Empty;

	[Description("Setting domain (optional)")]
	[CommandOption("-d|--domain <DOMAIN>")]
	public string? Domain { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(Setting))
			return ValidationResult.Error("Setting name is required");

		return ValidationResult.Success();
	}
}

public class IdbSettingsSetCommand : AsyncCommand<IdbSettingsSetCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbSettingsSetCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Setting [cyan]{settings.Setting}[/] to [cyan]{settings.Value}[/]...");

		try
		{
			await client.SetSettingAsync(settings.Setting, settings.Value, settings.Domain, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Setting updated successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbSettingsSetCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Setting name")]
	[CommandArgument(1, "<setting>")]
	public string Setting { get; set; } = string.Empty;

	[Description("Setting value")]
	[CommandArgument(2, "<value>")]
	public string Value { get; set; } = string.Empty;

	[Description("Setting domain (optional)")]
	[CommandOption("-d|--domain <DOMAIN>")]
	public string? Domain { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(Setting))
			return ValidationResult.Error("Setting name is required");

		if (string.IsNullOrWhiteSpace(Value))
			return ValidationResult.Error("Setting value is required");

		return ValidationResult.Success();
	}
}
