using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ValidateAppCommand : AsyncCommand<ValidateAppCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ValidateAppCommandSettings settings)
	{
		var data = context.GetData();
		var altool = new ALTool();

		try
		{
			if (!File.Exists(settings.AppPath) && !Directory.Exists(settings.AppPath))
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] App bundle not found at '{settings.AppPath}'");
				return this.ExitCode(false);
			}

			AnsiConsole.MarkupLine($"Validating [cyan]{Path.GetFileName(settings.AppPath)}[/]...");
			AnsiConsole.MarkupLine($"[dim]Type: {settings.AppType}[/]");

			await altool.ValidateAppAsync(
				settings.AppPath,
				settings.AppType,
				settings.KeyId,
				settings.IssuerId,
				data.CancellationToken);

			AnsiConsole.MarkupLine($"[green]✓ Validation successful for {Path.GetFileName(settings.AppPath)}[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class ValidateAppCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Path to .app or .ipa file")]
	[CommandArgument(0, "<app-path>")]
	public string AppPath { get; set; } = string.Empty;

	[Description("App type (ios, macos, watchos, tvos)")]
	[CommandOption("--type")]
	[DefaultValue(ALToolAppType.iOS)]
	[TypeConverter(typeof(StringEnumTypeConverter<ALToolAppType>))]
	public ALToolAppType AppType { get; set; } = ALToolAppType.iOS;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(AppPath))
		{
			return ValidationResult.Error("App path is required");
		}

		if (string.IsNullOrWhiteSpace(KeyId))
		{
			return ValidationResult.Error("App Store Connect API Key ID is required (--key-id or APP_STORE_CONNECT_KEY_ID env var)");
		}

		if (string.IsNullOrWhiteSpace(IssuerId))
		{
			return ValidationResult.Error("App Store Connect Issuer ID is required (--issuer-id or APP_STORE_CONNECT_ISSUER_ID env var)");
		}

		if (string.IsNullOrWhiteSpace(PrivateKey))
		{
			return ValidationResult.Error("App Store Connect Private Key is required (--private-key or APP_STORE_CONNECT_PRIVATE_KEY env var)");
		}

		return ValidationResult.Success();
	}
}
