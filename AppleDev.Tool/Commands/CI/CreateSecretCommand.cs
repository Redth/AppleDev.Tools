using System.ComponentModel;
using System.Text.RegularExpressions;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CreateSecretCommand : AsyncCommand<CreateSecretCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateSecretCommandSettings settings)
	{
		var data = context.GetData();

		var secret = string.Empty;

		if (settings.FromPrivateKey is not null)
		{
			var key = File.ReadAllText(settings.FromPrivateKey.FullName) ?? string.Empty;

			secret = Regex.Replace(key, @"\s+", "");
		}

		if (settings.FromCertificate is not null)
		{
			var c = File.ReadAllBytes(settings.FromCertificate.FullName);

			var b64 = Convert.ToBase64String(c);

			secret = Regex.Replace(b64, @"\s+", "");
		}

		AnsiConsole.WriteLine(secret);

		return this.ExitCode();
	}
}
public class CreateSecretCommandSettings : CommandSettings
{
	[Description("Certificate file (.p12 or .pfx)")]
	[CommandOption("--from-certificate <certificate_file>")]
	public FileInfo? FromCertificate { get; set; }

	[Description("Private Key file (.p8)")]
	[CommandOption("--from-private-key <private_key_file>")]
	public FileInfo? FromPrivateKey { get; set; }
}