using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Create a new certificate (auto-generates CSR)")]
    public static async Task<string> CreateCertificate(
        [Description("Certificate type (IOS_DEVELOPMENT, IOS_DISTRIBUTION, MAC_APP_DEVELOPMENT, DEVELOPER_ID_APPLICATION)")] string certificateType,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var certType = Enum.Parse<CertificateType>(certificateType, true);
        var response = await client.CreateCertificateAsync(certificateType: certType);

        if (response?.Data == null)
        {
            return "Failed to create certificate.";
        }

        return $"Certificate created successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"Type: {response.Data.Attributes.CertificateType}\n" +
               $"Serial Number: {response.Data.Attributes.SerialNumber}\n" +
               $"Certificate content length: {response.Data.Attributes.CertificateContent?.Length ?? 0} chars";
    }
}
