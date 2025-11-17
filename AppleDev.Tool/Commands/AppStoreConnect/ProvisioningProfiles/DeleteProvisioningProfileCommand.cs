using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeleteProvisioningProfileCommand : AsyncCommand<DeleteProvisioningProfileCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteProvisioningProfileCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var success = await appStoreConnect.DeleteProfileAsync(
			settings.Id,
			data.CancellationToken)
			.ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]Successfully deleted provisioning profile '{settings.Id}'[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Failed to delete provisioning profile '{settings.Id}'[/]");
			return this.ExitCode(false);
		}
	}
}

public class DeleteProvisioningProfileCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Provisioning profile ID to delete")]
	[CommandArgument(0, "<id>")]
	public string Id { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Id))
			return ValidationResult.Error("Provisioning profile ID is required");
		
		return base.Validate();
	}
}
