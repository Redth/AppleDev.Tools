using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Delete a provisioning profile")]
    public static async Task<string> DeleteProvisioningProfile(
        [Description("Provisioning profile ID to delete")] string profileId,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var success = await client.DeleteProfileAsync(profileId);

        return success
            ? $"Provisioning profile {profileId} deleted successfully!"
            : $"Failed to delete provisioning profile {profileId}.";
    }
}
