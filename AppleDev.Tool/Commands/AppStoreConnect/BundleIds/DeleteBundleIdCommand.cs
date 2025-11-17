using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeleteBundleIdCommand : AsyncCommand<DeleteBundleIdCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteBundleIdCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var success = await appStoreConnect.DeleteBundleIdAsync(
			settings.Id,
			data.CancellationToken)
			.ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]Successfully deleted bundle ID '{settings.Id}'[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Failed to delete bundle ID '{settings.Id}'[/]");
			return this.ExitCode(false);
		}
	}
}

public class DeleteBundleIdCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Bundle ID resource ID to delete")]
	[CommandArgument(0, "<id>")]
	public string Id { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Id))
			return ValidationResult.Error("Bundle ID resource ID is required");
		
		return base.Validate();
	}
}
