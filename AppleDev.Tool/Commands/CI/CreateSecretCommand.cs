using System.ComponentModel;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CreateSecretCommand : AsyncCommand<CreateSecretCommandSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, CreateSecretCommandSettings settings)
	{
		var secret = string.Empty;

		if (settings.FromPrivateKey is not null)
			secret = FromTextFile(settings.FromPrivateKey);
		else if (settings.FromTextFile is not null)
			secret = FromTextFile(settings.FromTextFile);
		else if (settings.FromCertificate is not null)
			secret = FromBinaryFile(settings.FromCertificate);
		else if (settings.FromKeystore is not null)
			secret = FromBinaryFile(settings.FromKeystore);
		else if (settings.FromPepk is not null)
			secret = FromBinaryFile(settings.FromPepk);
		else if (settings.FromBinaryFile is not null)
			secret = FromBinaryFile(settings.FromBinaryFile);

		AnsiConsole.WriteLine(secret);

		return Task.FromResult(this.ExitCode());
	}

	string FromTextFile(FileInfo file)
	{
		var key = File.ReadAllText(file.FullName) ?? string.Empty;

		return Regex.Replace(key, @"\s+", "");
	}

	string FromBinaryFile(FileInfo file)
	{
		var c = File.ReadAllBytes(file.FullName);

		var b64 = Convert.ToBase64String(c);

		return Regex.Replace(b64, @"\s+", "");
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

	[Description("Keystore file (.keystore)")]
	[CommandOption("--from-keystore <keystore_file>")]
	public FileInfo? FromKeystore { get; set; }

	[Description("Google PEPK Key file (.pepk)")]
	[CommandOption("--from-pepk <pepk_key_file>")]
	public FileInfo? FromPepk { get; set; }

	[Description("Text file")]
	[CommandOption("--from-text-file <text_file>")]
	public FileInfo? FromTextFile { get; set; }

	[Description("Binary file")]
	[CommandOption("--from-binary-file <binary_file>")]
	public FileInfo? FromBinaryFile { get; set; }
}