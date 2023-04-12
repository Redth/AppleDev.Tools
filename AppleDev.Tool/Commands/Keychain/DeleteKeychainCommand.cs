using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeleteKeychainCommand : AsyncCommand<DeleteKeychainCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteKeychainCommandSettings settings)
	{
		var data = context.GetData();
		var keychain = new Keychain();
		var success = await keychain.DeleteKeychainAsync(settings.Keychain, data.CancellationToken).ConfigureAwait(false);

		return this.ExitCode(success);
	}
}
public class DeleteKeychainCommandSettings : CommandSettings
{
	[Description("Keychain")]
	[DefaultValue("login.keychain-db")]
	[CommandOption("-k|--keychain <keychain>")]
	public string Keychain { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(Keychain))
		{
			return ValidationResult.Error("--keychain is required");
		}

		return base.Validate();
	}
}