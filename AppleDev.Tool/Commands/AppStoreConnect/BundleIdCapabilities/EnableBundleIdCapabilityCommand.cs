using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class EnableBundleIdCapabilityCommand : AsyncCommand<EnableBundleIdCapabilityCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, EnableBundleIdCapabilityCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		try
		{
			var response = await appStoreConnect.EnableCapabilityAsync(
				settings.BundleIdResourceId,
				settings.CapabilityType,
				cancellationToken: data.CancellationToken)
				.ConfigureAwait(false);

			if (response.Data is not null)
			{
				AnsiConsole.MarkupLine($"[green]Successfully enabled capability '{settings.CapabilityType}'[/]");
				AnsiConsole.MarkupLine($"  Capability ID: {response.Data.Id}");
				AnsiConsole.MarkupLine($"  Type: {response.Data.Attributes.CapabilityTypeValue}");
				return this.ExitCode();
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Failed to enable capability '{settings.CapabilityType}'[/]");
				return this.ExitCode(false);
			}
		}
		catch (AppleApiException ex)
		{
			AnsiConsole.MarkupLine($"[red]Failed to enable capability: {ex.Message}[/]");
			foreach (var error in ex.Errors)
			{
				AnsiConsole.MarkupLine($"  [red]{error.Title}: {error.Detail}[/]");
			}
			return this.ExitCode(false);
		}
	}
}

public class EnableBundleIdCapabilityCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("The resource ID of the bundle ID (use 'bundleids list' to find it)")]
	[CommandArgument(0, "<bundle-id-resource-id>")]
	public string BundleIdResourceId { get; set; } = string.Empty;

	[Description("The capability type to enable (e.g., PUSH_NOTIFICATIONS, ICLOUD, GAME_CENTER)")]
	[CommandArgument(1, "<capability-type>")]
	[TypeConverter(typeof(StringEnumTypeConverter<CapabilityType>))]
	public CapabilityType CapabilityType { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(BundleIdResourceId))
			return ValidationResult.Error("Bundle ID resource ID is required");

		if (CapabilityType == CapabilityType.Unknown)
			return ValidationResult.Error("A valid capability type is required");
		
		return base.Validate();
	}
}
