using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Install a provisioning profile locally")]
    public static async Task<string> InstallProvisioningProfile(
        [Description("Base64-encoded provisioning profile data (leave empty if using filePath)")] string? base64Data = null,
        [Description("Path to .mobileprovision or .provisionprofile file (leave empty if using base64Data)")] string? filePath = null)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrEmpty(base64Data) && string.IsNullOrEmpty(filePath))
        {
            return "Error: Either base64Data or filePath must be provided.";
        }

        if (!string.IsNullOrEmpty(base64Data) && !string.IsNullOrEmpty(filePath))
        {
            return "Error: Provide either base64Data or filePath, not both.";
        }

        try
        {
            byte[] profileData;

            if (!string.IsNullOrEmpty(base64Data))
            {
                // Install from base64 data
                profileData = Convert.FromBase64String(base64Data);
            }
            else
            {
                // Install from file
                if (!File.Exists(filePath))
                {
                    return $"Error: File not found at path: {filePath}";
                }

                profileData = await File.ReadAllBytesAsync(filePath);
            }

            // Parse the profile to get details before installing
            var profileInfo = await ProvisioningProfiles.ParseAsync(profileData);

            // Install the profile
            await ProvisioningProfiles.InstallProfileAsync(profileData);

            var directory = await ProvisioningProfiles.GetDirectory();
            var extension = profileInfo.Platform.Any(p => p == "MacOS")
                ? "provisionprofile"
                : "mobileprovision";
            var installedPath = Path.Combine(directory.FullName, $"{profileInfo.Uuid}.{extension}");

            return $"Provisioning profile installed successfully!\n\n" +
                   $"Name: {profileInfo.Name}\n" +
                   $"UUID: {profileInfo.Uuid}\n" +
                   $"Team: {profileInfo.TeamName}\n" +
                   $"Platform: {string.Join(", ", profileInfo.Platform)}\n" +
                   $"Expiration: {profileInfo.ExpirationDate:yyyy-MM-dd HH:mm:ss}\n" +
                   $"Installed Path: {installedPath}";
        }
        catch (FormatException)
        {
            return "Error: Invalid base64 data provided.";
        }
        catch (Exception ex)
        {
            return $"Error installing provisioning profile: {ex.Message}";
        }
    }
}
