using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CreateKeychainCommand : AsyncCommand<CreateKeychainCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateKeychainCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var keychain = new Keychain();
		var result = await keychain.CreateKeychainAsync(settings.Keychain, settings.Password, data.CancellationToken).ConfigureAwait(false);

		if (!result.Success)
			result.OutputFailure("Create Keychain Failed");

		return this.ExitCode(result.Success);
	}
}
public class CreateKeychainCommandSettings : CommandSettings
{
	[Description("Keychain password")]
	[CommandOption("-p|--password <password>")]
	public string Password { get; set; } = string.Empty;

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

		if (string.IsNullOrEmpty(Password))
		{
			return ValidationResult.Error("--password is required");
		}

		return base.Validate();
	}

}