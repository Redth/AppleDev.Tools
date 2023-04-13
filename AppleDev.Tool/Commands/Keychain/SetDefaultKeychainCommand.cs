using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class SetDefaultKeychainCommand : AsyncCommand<SetDefaultKeychainCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, SetDefaultKeychainCommandSettings settings)
    {
        var data = context.GetData();
        var keychain = new Keychain();
        var result = await keychain.SetDefaultKeychainAsync(settings.Keychain, data.CancellationToken).ConfigureAwait(false);
		
        if (!result.Success)
            result.OutputFailure("Set Default Keychain Failed");
		
        return this.ExitCode(result.Success);
    }
}
public class SetDefaultKeychainCommandSettings : CommandSettings
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