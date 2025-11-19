using System.ComponentModel;
using System.Text;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List bundle IDs with optional filtering")]
    public static async Task<string> ListBundleIds(
        [Description("Filter by bundle identifier (e.g., 'com.example.app')")] string? identifier = null,
        [Description("Filter by platform (IOS, MAC_OS, etc.)")] string? platform = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var filterIdentifier = identifier != null ? new[] { identifier } : null;
        var filterPlatform = platform != null ? new[] { Enum.Parse<Platform>(platform, true) } : null;

        var response = await client.ListBundleIdsAsync(
            filterIdentifier: filterIdentifier,
            filterPlatform: filterPlatform);

        if (response?.Data == null || !response.Data.Any())
        {
            return "No bundle IDs found matching the criteria.";
        }

        var result = new StringBuilder("Bundle IDs:\n\n");
        foreach (var bundleId in response.Data)
        {
            result.AppendLine($"ID: {bundleId.Id}");
            result.AppendLine($"  Name: {bundleId.Attributes.Name}");
            result.AppendLine($"  Identifier: {bundleId.Attributes.Identifier}");
            result.AppendLine($"  Platform: {bundleId.Attributes.Platform}");
            if (!string.IsNullOrEmpty(bundleId.Attributes.SeedId))
                result.AppendLine($"  Seed ID: {bundleId.Attributes.SeedId}");
            result.AppendLine();
        }

        return result.ToString();
    }
}
