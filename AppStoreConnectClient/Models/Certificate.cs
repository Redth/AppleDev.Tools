using System;
using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class CertificateAttributes
{
	public CertificateAttributes() { }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("certificateContent")]
	public string CertificateContent { get; set; } = string.Empty;

	[JsonPropertyName("displayName")]
	public string DisplayName { get; set; } = string.Empty;

	[JsonPropertyName("platform")]
	public string PlatformValue { get; set; } = string.Empty;

	[JsonIgnore]
	public Platform Platform
	{
		get => Enum.TryParse<Platform>(PlatformValue, out var v) ? v : Platform.Unknown;
		set => value.ToString();
	}

	[JsonPropertyName("serialNumber")]
	public string SerialNumber { get; set; } = string.Empty;

	[JsonPropertyName("certificateType")]
	public string CertificateTypeValue { get; set; } = string.Empty;

	[JsonIgnore]
	public CertificateType CertificateType
	{
		get => Enum.TryParse<CertificateType>(CertificateTypeValue, out var v) ? v : CertificateType.Unknown;
		set => value.ToString();
	}
}

public class Certificate : Item<CertificateAttributes>
{
	public const string TYPE = "certificates";

	public Certificate()
		: base(new CertificateAttributes())
	{
	}

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class CertificateResponse : ListResponse<Certificate, CertificateAttributes>
{
	public CertificateResponse() { }
}

public class CreateCertificateRequestAttributes
{
	public CreateCertificateRequestAttributes(string csrContent, string certificateType)
	{
		CsrContent = csrContent;
		CertificateTypeValue = certificateType;
	}

	public CreateCertificateRequestAttributes(string csrContent, CertificateType certificateType)
	{
		CsrContent = csrContent;
		CertificateType = certificateType;
	}

	[JsonPropertyName("csrContent")]
	public string CsrContent { get; set; } = string.Empty;

	[JsonPropertyName("certificateType")]
	public string CertificateTypeValue { get; set; } = string.Empty;

	[JsonIgnore]
	public CertificateType CertificateType
	{
		get => Enum.TryParse<CertificateType>(CertificateTypeValue, out var v) ? v : CertificateType.Unknown;
		set => value.ToString();
	}
}

public enum CertificateType
{
	Unknown,
	IOS_DEVELOPMENT,
	IOS_DISTRIBUTION,
	MAC_APP_DISTRIBUTION,
	MAC_INSTALLER_DISTRIBUTION,
	MAC_APP_DEVELOPMENT,
	DEVELOPER_ID_KEXT,
	DEVELOPER_ID_APPLICATION,
	DEVELOPMENT,
	DISTRIBUTION,
	PASS_TYPE_ID,
	PASS_TYPE_ID_WITH_NFC,
}