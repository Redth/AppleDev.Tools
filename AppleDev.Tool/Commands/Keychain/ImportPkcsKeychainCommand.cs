using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ImportPkcs12KeychainCommand : AsyncCommand<ImportPkcs12KeychainCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ImportPkcs12KeychainCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var keychain = new Keychain();
		var result = await keychain.ImportPkcs12Async(settings.CertificateFile!.FullName, settings.CertificatePassphrase, settings.Keychain, settings.AllowAnyAppRead, data.CancellationToken).ConfigureAwait(false);

		if (!result.Success)
			result.OutputFailure("Import Keychain Failed");

		return this.ExitCode(result.Success);
	}
}

public class ImportPkcs12KeychainCommandSettings : CommandSettings
{
	[Description("Certificate file (PKCS12/PFX) to import")]
	[CommandArgument(0, "<certificate_file>")]
	[TypeConverter(typeof(FileInfoTypeConverter))]
	public FileInfo? CertificateFile { get; set; }

	[Description("Certificate's passphrase")]
	[CommandOption("-p|--passphrase <passphrase>")]
	public string CertificatePassphrase { get; set; } = string.Empty;

	[Description("Keychain name to import into")]
	[DefaultValue(AppleDev.Keychain.DefaultKeychain)]
	[CommandOption("-k|--keychain <keychain>")]
	public string Keychain { get; set; } = AppleDev.Keychain.DefaultKeychain;

	[Description("Allows any app read permission")]
	[CommandOption("--allow-any-app-read")]
	public bool AllowAnyAppRead { get; set; }


	public override ValidationResult Validate()
	{
		if (CertificateFile is null || !CertificateFile.Exists)
			return ValidationResult.Error("--certificate is required");

		
		return base.Validate();
	}
}