using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class UnlockKeychainCommand : AsyncCommand<UnlockKeychainCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, UnlockKeychainCommandSettings settings)
	{
		var data = context.GetData();
		var keychain = new Keychain();
		var result = await keychain.UnlockKeychainAsync(settings.Password, settings.Keychain, data.CancellationToken).ConfigureAwait(false);

		if (!result.Success)
			result.OutputFailure("Unlock Keychain Failed");

		return this.ExitCode(result.Success);
	}
}
public class UnlockKeychainCommandSettings : CommandSettings
{
	[Description("Keychain password")]
	[CommandOption("-p|--password <password>")]
	public string Password { get; set; } = string.Empty;

	[Description("Keychain name")]
	[DefaultValue(AppleDev.Keychain.DefaultKeychain)]
	[CommandOption("-k|--keychain <keychain>")]
	public string Keychain { get; set; } = AppleDev.Keychain.DefaultKeychain;
}