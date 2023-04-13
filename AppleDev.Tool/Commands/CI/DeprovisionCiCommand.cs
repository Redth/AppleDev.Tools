using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeprovisionCiCommand : AsyncCommand<DeprovisionCiCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeprovisionCiCommandSettings settings)
	{
		var data = context.GetData();
		
		AnsiConsole.Write(new Rule("Deprovisioning..."));
		
		if (!string.IsNullOrEmpty(settings.Keychain) && settings.Keychain != Keychain.DefaultKeychain)
		{
			var keychain = new Keychain();
			var keychainFile = keychain.Locate(settings.Keychain);

			AnsiConsole.Write($"Deleting Keychain {keychainFile.Name}...");
			var createResult = await keychain.DeleteKeychainAsync(settings.Keychain, data.CancellationToken).ConfigureAwait(false);

			if (!createResult.Success)
			{
				AnsiConsole.WriteLine();
				createResult.OutputFailure("Deleting Keychain Failed");
				return 1;
			}

			AnsiConsole.WriteLine($" Done.");
		}

		return this.ExitCode();
	}
}
public class DeprovisionCiCommandSettings : CommandSettings
{
	[Description("Keychain name to import into")]
	[DefaultValue("login.keychain-db")]
	[CommandOption("--keychain <keychain>")]
	public string Keychain { get; set; } = AppleDev.Keychain.DefaultKeychain;
}