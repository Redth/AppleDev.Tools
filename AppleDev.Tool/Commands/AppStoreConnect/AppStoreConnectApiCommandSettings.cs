using System.ComponentModel;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class AppStoreConnectApiCommandSettings : CommandSettings
{
    [Description("AppStoreConnect Key ID")]
    [CommandOption("-k|--key-id <key_id>")]
    public string KeyId { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_KEY_ID") ?? string.Empty;

    [Description("AppStoreConnect Issuer ID")]
    [CommandOption("-i|--issuer-id <issuer_id>")]
    public string IssuerId { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_ISSUER_ID") ?? string.Empty;

    [Description("AppStoreConnect Private Key Filename or Base64 string")]
    [CommandOption("-p|--private-key <private_key>")]
    public string PrivateKey { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_PRIVATE_KEY") ?? string.Empty;

	internal string GetPrivateKeyBase64()
    {
		if (File.Exists(PrivateKey))
        {
            return File.ReadAllText(PrivateKey);
        }

        return PrivateKey;
    }
}

public class FormattableOutputAppStoreConnectCommand : FormattableOutputCommandSettings
{
	[Description("AppStoreConnect Key ID")]

	[CommandOption("-k|--key-id <key_id>")]
	public string KeyId { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_KEY_ID") ?? string.Empty;

	[Description("AppStoreConnect Issuer ID")]
	[CommandOption("-i|--issuer-id <issuer_id>")]
	public string IssuerId { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_ISSUER_ID") ?? string.Empty;

	[Description("AppStoreConnect Private Key Filename or Base64 string")]
	[CommandOption("-p|--private-key <private_key>")]
	public string PrivateKey { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_PRIVATE_KEY") ?? string.Empty;

	internal string GetPrivateKeyBase64()
	{
		if (File.Exists(PrivateKey))
		{
			return File.ReadAllText(PrivateKey);
		}

		return PrivateKey;
	}
}