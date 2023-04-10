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
		var success = await keychain.UnlockKeychainAsync(settings.Password, settings.Keychain, data.CancellationToken).ConfigureAwait(false);

		return this.ExitCode(success);
	}
}
public class UnlockKeychainCommandSettings : CommandSettings
{
	[Description("Keychain password")]
	[CommandOption("-p|-password <password>")]
	public string Password { get; set; } = string.Empty;

	[Description("Keychain name")]
	[DefaultValue("login.keychain-db")]
	[CommandOption("-k|-keychain <keychain>")]
	public string Keychain { get; set; } = "login.keychain-db";
}