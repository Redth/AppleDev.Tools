using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class RevokeCertificateCommand : AsyncCommand<RevokeCertificateCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, RevokeCertificateCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var success = await appStoreConnect.RevokeCertificateAsync(
			settings.Id,
			data.CancellationToken)
			.ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]Successfully revoked certificate '{settings.Id}'[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Failed to revoke certificate '{settings.Id}'[/]");
			return this.ExitCode(false);
		}
	}
}

public class RevokeCertificateCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Certificate ID to revoke")]
	[CommandArgument(0, "<id>")]
	public string Id { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Id))
			return ValidationResult.Error("Certificate ID is required");
		
		return base.Validate();
	}
}
