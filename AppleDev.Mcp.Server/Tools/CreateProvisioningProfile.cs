using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Create a new provisioning profile")]
    public static async Task<string> CreateProvisioningProfile(
        [Description("Profile name")] string name,
        [Description("Profile type (IOS_APP_DEVELOPMENT, IOS_APP_STORE, IOS_APP_ADHOC, MAC_APP_DEVELOPMENT)")] string profileType,
        [Description("Bundle ID (the ID, not the identifier string)")] string bundleIdId,
        [Description("Comma-separated list of certificate IDs to include")] string certificateIds,
        [Description("Comma-separated list of device IDs (optional, not needed for App Store profiles)")] string? deviceIds = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var certIds = certificateIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var devIds = deviceIds?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var profileTypeEnum = Enum.Parse<ProfileType>(profileType, true);
        var response = await client.CreateProfileAsync(name, profileTypeEnum, bundleIdId, certIds, devIds);

        if (response?.Data == null)
        {
            return "Failed to create provisioning profile.";
        }

        return $"Provisioning profile created successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"Type: {response.Data.Attributes.ProfileType}\n" +
               $"UUID: {response.Data.Attributes.Uuid}\n" +
               $"State: {response.Data.Attributes.ProfileState}\n" +
               $"Created: {response.Data.Attributes.CreatedDate}\n" +
               $"Expiration: {response.Data.Attributes.ExpirationDate}";
    }
}
