using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Revoke (delete) a certificate")]
    public static async Task<string> RevokeCertificate(
        [Description("Certificate ID to revoke")] string certificateId,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var success = await client.RevokeCertificateAsync(certificateId);

        return success
            ? $"Certificate {certificateId} revoked successfully!"
            : $"Failed to revoke certificate {certificateId}.";
    }
}
