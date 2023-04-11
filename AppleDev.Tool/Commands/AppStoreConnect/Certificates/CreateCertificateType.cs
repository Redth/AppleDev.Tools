using System.Runtime.Serialization;

namespace AppleDev.Tool.Commands;

public enum CreateCertificateType
{
	[EnumMember(Value = "IOS_DEVELOPMENT")]
	IOS_DEVELOPMENT = 1,

	[EnumMember(Value = "IOS_DISTRIBUTION")]
	IOS_DISTRIBUTION = 2,

	[EnumMember(Value = "MAC_APP_DISTRIBUTION")]
	MAC_APP_DISTRIBUTION = 3,

	[EnumMember(Value = "MAC_INSTALLER_DISTRIBUTION")]
	MAC_INSTALLER_DISTRIBUTION = 4,

	[EnumMember(Value = "MAC_APP_DEVELOPMENT")]
	MAC_APP_DEVELOPMENT = 5,

	[EnumMember(Value = "DEVELOPER_ID_KEXT")]
	DEVELOPER_ID_KEXT = 6,

	[EnumMember(Value = "DEVELOPER_ID_APPLICATION")]
	DEVELOPER_ID_APPLICATION = 7,

	[EnumMember(Value = "DEVELOPMENT")]
	DEVELOPMENT = 8,

	[EnumMember(Value = "DISTRIBUTION")]
	DISTRIBUTION = 9,

	[EnumMember(Value = "PASS_TYPE_ID")]
	PASS_TYPE_ID = 10,

	[EnumMember(Value = "PASS_TYPE_ID_WITH_NFC")]
	PASS_TYPE_ID_WITH_NFC = 11
}
