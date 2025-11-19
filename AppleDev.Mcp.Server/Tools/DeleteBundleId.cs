using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Delete a bundle ID")]
    public static async Task<string> DeleteBundleId(
        [Description("Bundle ID to delete")] string bundleIdId,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var success = await client.DeleteBundleIdAsync(bundleIdId);

        return success
            ? $"Bundle ID {bundleIdId} deleted successfully!"
            : $"Failed to delete bundle ID {bundleIdId}.";
    }
}
