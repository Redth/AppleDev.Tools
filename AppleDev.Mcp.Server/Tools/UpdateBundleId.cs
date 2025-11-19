using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Update an existing bundle ID's name")]
    public static async Task<string> UpdateBundleId(
        [Description("Bundle ID to update")] string bundleIdId,
        [Description("New display name")] string name,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var attributes = new BundleIdAttributes { Name = name };
        var response = await client.UpdateBundleIdAsync(bundleIdId, attributes);

        if (response?.Data == null)
        {
            return "Failed to update bundle ID.";
        }

        return $"Bundle ID updated successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"Identifier: {response.Data.Attributes.Identifier}";
    }
}
