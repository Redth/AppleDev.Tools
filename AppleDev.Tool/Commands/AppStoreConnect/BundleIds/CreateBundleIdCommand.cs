using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CreateBundleIdCommand : AsyncCommand<CreateBundleIdCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateBundleIdCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var bundleIdAttributes = new BundleIdAttributes
		{
			Name = settings.Name,
			Identifier = settings.Identifier,
			PlatformValue = settings.Platform.ToString(),
			SeedId = settings.SeedId ?? string.Empty
		};

		var response = await appStoreConnect.CreateBundleIdAsync(
			bundleIdAttributes,
			data.CancellationToken)
			.ConfigureAwait(false);

		if (response.Data != null)
		{
			AnsiConsole.MarkupLine($"[green]Successfully created bundle ID '{settings.Name}'[/]");
			AnsiConsole.MarkupLine($"  ID: {response.Data.Id}");
			AnsiConsole.MarkupLine($"  Identifier: {response.Data.Attributes.Identifier}");
			AnsiConsole.MarkupLine($"  Platform: {response.Data.Attributes.PlatformValue}");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Failed to create bundle ID '{settings.Name}'[/]");
			return this.ExitCode(false);
		}
	}
}

public class CreateBundleIdCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Display name for the bundle ID")]
	[CommandArgument(0, "<name>")]
	public string Name { get; set; } = string.Empty;

	[Description("Bundle identifier (e.g., com.mycompany.myapp)")]
	[CommandArgument(1, "<identifier>")]
	public string Identifier { get; set; } = string.Empty;

	[Description("Platform (IOS, MAC_OS, UNIVERSAL)")]
	[CommandArgument(2, "<platform>")]
	[TypeConverter(typeof(StringEnumTypeConverter<Platform>))]
	public Platform Platform { get; set; } = Platform.IOS;

	[Description("App ID Prefix / Seed ID (optional, defaults to Team ID)")]
	[CommandOption("--seed-id <seedid>")]
	public string? SeedId { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Name))
			return ValidationResult.Error("Bundle ID name is required");
		
		if (string.IsNullOrWhiteSpace(Identifier))
			return ValidationResult.Error("Bundle identifier is required");

		// Validate identifier format
		if (Identifier.Contains("*") && !Identifier.EndsWith(".*"))
			return ValidationResult.Error("Wildcard bundle identifiers must end with '.*' (e.g., com.example.*)");
		
		return base.Validate();
	}
}
