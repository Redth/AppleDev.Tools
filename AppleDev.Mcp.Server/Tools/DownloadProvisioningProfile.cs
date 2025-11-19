using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Download a provisioning profile from App Store Connect")]
    public static async Task<string> DownloadProvisioningProfile(
        [Description("Profile ID from App Store Connect")] string profileId,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        try
        {
            var client = GetClient(keyName);

            // Get the profile by ID with profileContent included
            var response = await client.ListProfilesAsync(
                filterId: new[] { profileId },
                fieldsProfiles: new[] { "profileContent", "name", "uuid", "profileType", "profileState" });

            if (response?.Data == null || !response.Data.Any())
            {
                return $"Profile with ID '{profileId}' not found.";
            }

            var profile = response.Data.First();

            if (string.IsNullOrEmpty(profile.Attributes.ProfileContent))
            {
                return $"Profile '{profile.Attributes.Name}' does not have content available.";
            }

            return $"Profile: {profile.Attributes.Name}\n" +
                   $"UUID: {profile.Attributes.Uuid}\n" +
                   $"Type: {profile.Attributes.ProfileType}\n" +
                   $"State: {profile.Attributes.ProfileState}\n" +
                   $"\nBase64 Content:\n{profile.Attributes.ProfileContent}";
        }
        catch (AppleApiException ex)
        {
            return $"App Store Connect API Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error downloading provisioning profile: {ex.Message}";
        }
    }
}
