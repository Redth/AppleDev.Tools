using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List locally installed provisioning profiles")]
    public static async Task<string> ListInstalledProvisioningProfiles()
    {
        var profiles = await ProvisioningProfiles.ListProfilesAsync();

        if (profiles == null || !profiles.Any())
        {
            return "No installed provisioning profiles found.";
        }

        var result = new StringBuilder("Installed Provisioning Profiles:\n\n");
        foreach (var profile in profiles)
        {
            result.AppendLine($"Name: {profile.Name}");
            result.AppendLine($"  UUID: {profile.Uuid}");
            result.AppendLine($"  Team Name: {profile.TeamName}");
            result.AppendLine($"  App ID Name: {profile.AppIdName}");
            result.AppendLine($"  Platform: {string.Join(", ", profile.Platform)}");
            result.AppendLine($"  Creation Date: {profile.CreationDate:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine($"  Expiration Date: {profile.ExpirationDate:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine($"  Is Xcode Managed: {profile.IsXcodeManaged}");
            if (profile.ProvisionedDevices?.Any() == true)
                result.AppendLine($"  Provisioned Devices: {profile.ProvisionedDevices.Length}");
            result.AppendLine();
        }

        return result.ToString();
    }
}
