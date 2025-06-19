using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using AppleAppStoreConnect;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ALToolInstallPrivateKeyCommand : AsyncCommand<ALToolInstallPrivateKeyCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ALToolInstallPrivateKeyCommandSettings settings)
	{
		var altool = new ALTool();

		await altool.InstallPrivateKey(settings.KeyId, settings.GetPrivateKeyBase64()).ConfigureAwait(false);

		return this.ExitCode();
	}
}

public class ALToolInstallPrivateKeyCommandSettings : AppStoreConnectApiCommandSettings
{
	public override ValidationResult Validate()
	{
		
		return base.Validate();
	}
}
