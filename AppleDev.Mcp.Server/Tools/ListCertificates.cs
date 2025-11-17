using System.ComponentModel;
using System.Text;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List certificates with optional filtering")]
    public static async Task<string> ListCertificates(
        [Description("Filter by certificate type (e.g., IOS_DEVELOPMENT, IOS_DISTRIBUTION)")] string? certificateType = null,
        [Description("Filter by display name")] string? displayName = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var filterType = certificateType != null ? new[] { Enum.Parse<CertificateType>(certificateType, true) } : null;
        var filterDisplayName = displayName != null ? new[] { displayName } : null;

        var response = await client.ListCertificatesAsync(
            filterCertificateType: filterType,
            filterDisplayName: filterDisplayName);

        if (response?.Data == null || !response.Data.Any())
        {
            return "No certificates found matching the criteria.";
        }

        var result = new StringBuilder("Certificates:\n\n");
        foreach (var cert in response.Data)
        {
            result.AppendLine($"ID: {cert.Id}");
            result.AppendLine($"  Name: {cert.Attributes.Name}");
            result.AppendLine($"  Type: {cert.Attributes.CertificateType}");
            if (!string.IsNullOrEmpty(cert.Attributes.SerialNumber))
                result.AppendLine($"  Serial Number: {cert.Attributes.SerialNumber}");
            if (!string.IsNullOrEmpty(cert.Attributes.PlatformValue))
                result.AppendLine($"  Platform: {cert.Attributes.Platform}");
            if (!string.IsNullOrEmpty(cert.Attributes.DisplayName))
                result.AppendLine($"  Display Name: {cert.Attributes.DisplayName}");
            result.AppendLine();
        }

        return result.ToString();
    }
}
