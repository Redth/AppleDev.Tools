using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CreateCertificateCommand : AsyncCommand<CreateCertificateCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateCertificateCommandSettings settings)
	{
		//var data = context.GetData();
		var config = new AppStoreConnect.Client.AppStoreConnectConfiguration(settings.KeyId, settings.IssuerId, settings.GetPrivateKeyBase64());
		var api = new AppStoreConnect.Api.CertificatesApi(config);

		var csrGenerator = new CertificateSigningRequestGenerator();
		var csr = csrGenerator.GeneratePem(settings.CommonName);

		var response = await api.CertificatesCreateInstanceAsync(new AppStoreConnect.Model.CertificateCreateRequest(new AppStoreConnect.Model.CertificateCreateRequestData(AppStoreConnect.Model.CertificateCreateRequestData.TypeEnum.Certificates,
			new AppStoreConnect.Model.CertificateCreateRequestDataAttributes(
				csr,
				settings.CertificateType switch
				{
					CreateCertificateType.IOS_DEVELOPMENT => AppStoreConnect.Model.CertificateType.IOSDEVELOPMENT,
					CreateCertificateType.IOS_DISTRIBUTION => AppStoreConnect.Model.CertificateType.IOSDISTRIBUTION,
					CreateCertificateType.MAC_APP_DISTRIBUTION => AppStoreConnect.Model.CertificateType.MACAPPDISTRIBUTION,
					CreateCertificateType.MAC_INSTALLER_DISTRIBUTION => AppStoreConnect.Model.CertificateType.MACINSTALLERDISTRIBUTION,
					CreateCertificateType.MAC_APP_DEVELOPMENT => AppStoreConnect.Model.CertificateType.MACAPPDEVELOPMENT,
					CreateCertificateType.DEVELOPER_ID_KEXT => AppStoreConnect.Model.CertificateType.DEVELOPERIDKEXT,
					CreateCertificateType.DEVELOPER_ID_APPLICATION => AppStoreConnect.Model.CertificateType.DEVELOPERIDAPPLICATION,
					CreateCertificateType.DEVELOPMENT => AppStoreConnect.Model.CertificateType.DEVELOPMENT,
					CreateCertificateType.DISTRIBUTION => AppStoreConnect.Model.CertificateType.DISTRIBUTION,
					CreateCertificateType.PASS_TYPE_ID => AppStoreConnect.Model.CertificateType.PASSTYPEID,
					CreateCertificateType.PASS_TYPE_ID_WITH_NFC => AppStoreConnect.Model.CertificateType.PASSTYPEIDWITHNFC,
					_ => AppStoreConnect.Model.CertificateType.DEVELOPMENT,
				})))).ConfigureAwait(false);

		var cert = new X509Certificate2(Convert.FromBase64String(response.Data.Attributes.CertificateContent));

		var pfxData = cert.Export(X509ContentType.Pfx, settings.Passphrase);

		var outputFile = settings.GetOutputFile("certificate", ".pfx").FullName;

		File.WriteAllBytes(outputFile, pfxData);

		var result = new CreateCertificateResultOutput
		{
			Path = outputFile,
			Expiration = cert.NotAfter,
		};

		OutputHelper.OutputObject(result, false,
			new ColumnInfo<CreateCertificateResultOutput>("Certificate Path", r => r.Path),
			new ColumnInfo<CreateCertificateResultOutput>("Expiration", r => r.Expiration.ToUnixTimeMilliseconds().ToString()));

		return this.ExitCode();
	}

	class CreateCertificateResultOutput
	{
		public string Path { get; set; } = string.Empty;

		public DateTimeOffset Expiration { get; set; } = DateTimeOffset.UtcNow;
	}
}
public class CreateCertificateCommandSettings : AppStoreConnectApiCommandSettings, IOutputCommandSettings
{
	[Description("Type of Apple certificate")]
	[CommandOption("-t|--type <type>")]
	public CreateCertificateType CertificateType { get; set; } = CreateCertificateType.DEVELOPMENT;

	[Description("Optional common name for the CSR")]
	[CommandOption("-c|--common-name <common-name>")]
	public string? CommonName { get; set; }

	[Description("Optional passphrase for the output pfx/pkcs12")]
	[CommandOption("--passphrase <passphrase>")]
	public string? Passphrase { get; set; }

	[Description("Output path (file or directory) to save the certificate pfx/pkcs12")]
	[CommandOption("-o|--output <PATH>")]
	public string? Output { get; set; }

	public override ValidationResult Validate()
	{
		if (!this.IsValid(out var r))
			return r;

		return base.Validate();
	}
}
