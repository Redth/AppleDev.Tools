using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class UpdateBundleIdCommand : AsyncCommand<UpdateBundleIdCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, UpdateBundleIdCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.KeyId, settings.IssuerId, settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var bundleIdAttributes = new BundleIdAttributes
		{
			Name = settings.Name
		};

		var response = await appStoreConnect.UpdateBundleIdAsync(
			settings.Id,
			bundleIdAttributes,
			data.CancellationToken)
			.ConfigureAwait(false);

		if (response.Data != null)
		{
			AnsiConsole.MarkupLine($"[green]Successfully updated bundle ID '{settings.Id}'[/]");
			AnsiConsole.MarkupLine($"  Name: {response.Data.Attributes.Name}");
			AnsiConsole.MarkupLine($"  Identifier: {response.Data.Attributes.Identifier}");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Failed to update bundle ID '{settings.Id}'[/]");
			return this.ExitCode(false);
		}
	}
}

public class UpdateBundleIdCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Bundle ID resource ID to update")]
	[CommandArgument(0, "<id>")]
	public string Id { get; set; } = string.Empty;

	[Description("New display name for the bundle ID")]
	[CommandOption("--name <name>")]
	public string Name { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Id))
			return ValidationResult.Error("Bundle ID resource ID is required");
		
		if (string.IsNullOrWhiteSpace(Name))
			return ValidationResult.Error("--name is required");
		
		return base.Validate();
	}
}
