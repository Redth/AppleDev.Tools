using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DeleteKeychainCommand : AsyncCommand<DeleteKeychainCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeleteKeychainCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var keychain = new Keychain();
		var result = await keychain.DeleteKeychainAsync(settings.Keychain, data.CancellationToken).ConfigureAwait(false);
		
		if (!result.Success)
			result.OutputFailure("Delete Keychain Failed");
		
		return this.ExitCode(result.Success);
	}
}
public class DeleteKeychainCommandSettings : CommandSettings
{
	[Description("Keychain")]
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