using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListCertificatesCommand : AsyncCommand<ListCertificatesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListCertificatesCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var certificates = await appStoreConnect.ListCertificatesAsync(
			filterId: settings.Ids.Length > 0 ? settings.Ids : null,
			filterDisplayName: settings.DisplayNames.Length > 0 ? settings.DisplayNames : null,
			filterSerialNumber: settings.SerialNumbers.Length > 0 ? settings.SerialNumbers : null,
			filterCertificateType: settings.CertificateTypes.Length > 0 ? settings.CertificateTypes : null,
			limit: 200,
			cancellationToken: data.CancellationToken)
			.ConfigureAwait(false);
		
		var certificateResults = new List<CertificateInfo>();

		foreach (var cert in certificates.Data)
		{
			certificateResults.Add(new CertificateInfo
			{
				Id = cert.Id,
				Name = cert.Attributes.Name,
				DisplayName = cert.Attributes.DisplayName,
				CertificateType = cert.Attributes.CertificateTypeValue,
				SerialNumber = cert.Attributes.SerialNumber,
				Platform = cert.Attributes.PlatformValue
			});
		}

		OutputHelper.Output(certificateResults, settings.Format, settings.Verbose,
			new ColumnInfo<CertificateInfo>("Display Name", r => r.DisplayName),
			new ColumnInfo<CertificateInfo>("Type", r => r.CertificateType),
			new ColumnInfo<CertificateInfo>("Platform", r => r.Platform),
			new ColumnInfo<CertificateInfo>("Serial Number", r => r.SerialNumber),
			new ColumnInfo<CertificateInfo>("Name", r => r.Name, true),
			new ColumnInfo<CertificateInfo>("Id", r => r.Id, true));

		return this.ExitCode();
	}

	class CertificateInfo
	{
		public string Id { get; set; } = string.Empty;
		public string? Name { get; set; }
		public string? DisplayName { get; set; }
		public string CertificateType { get; set; } = string.Empty;
		public string? SerialNumber { get; set; }
		public string? Platform { get; set; }
	}
}

public class ListCertificatesCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Filter by certificate ID")]
	[CommandOption("--id <id>")]
	public string[] Ids { get; set; } = Array.Empty<string>();

	[Description("Filter by display name")]
	[CommandOption("--display-name <name>")]
	public string[] DisplayNames { get; set; } = Array.Empty<string>();

	[Description("Filter by serial number")]
	[CommandOption("--serial-number <serial>")]
	public string[] SerialNumbers { get; set; } = Array.Empty<string>();

	[Description("Filter by certificate type")]
	[CommandOption("--type <type>")]
	[TypeConverter(typeof(StringEnumTypeConverter<CertificateType>))]
	public CertificateType[] CertificateTypes { get; set; } = Array.Empty<CertificateType>();
}
