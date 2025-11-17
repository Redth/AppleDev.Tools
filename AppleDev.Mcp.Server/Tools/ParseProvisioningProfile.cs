using System.ComponentModel;
using System.Text;
using System.Text.Json;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Parse and display provisioning profile contents")]
    public static async Task<string> ParseProvisioningProfile(
        [Description("Path to the provisioning profile file (.mobileprovision or .provisionprofile)")] string profilePath)
    {
        if (!File.Exists(profilePath))
        {
            return $"Error: File not found at path: {profilePath}";
        }

        try
        {
            var fileData = await File.ReadAllBytesAsync(profilePath);
            var profile = await ProvisioningProfiles.ParseAsync(fileData);

            var result = new StringBuilder("Provisioning Profile Details:\n\n");
            result.AppendLine($"Name: {profile.Name}");
            result.AppendLine($"UUID: {profile.Uuid}");
            result.AppendLine($"Team Name: {profile.TeamName}");
            result.AppendLine($"Team Identifier: {string.Join(", ", profile.TeamIdentifier)}");
            result.AppendLine($"App ID Name: {profile.AppIdName}");
            result.AppendLine($"Application Identifier Prefix: {string.Join(", ", profile.ApplicationIdentifierPrefix)}");
            result.AppendLine($"Platform: {string.Join(", ", profile.Platform)}");
            result.AppendLine($"Creation Date: {profile.CreationDate:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine($"Expiration Date: {profile.ExpirationDate:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine($"Time To Live: {profile.TimeToLive} days");
            result.AppendLine($"Version: {profile.Version}");
            result.AppendLine($"Is Xcode Managed: {profile.IsXcodeManaged}");
            
            if (profile.ProvisionedDevices?.Any() == true)
            {
                result.AppendLine($"\nProvisioned Devices ({profile.ProvisionedDevices.Length}):");
                foreach (var device in profile.ProvisionedDevices)
                {
                    result.AppendLine($"  - {device}");
                }
            }

            if (profile.DeveloperCertificates?.Any() == true)
            {
                result.AppendLine($"\nDeveloper Certificates: {profile.DeveloperCertificates.Length}");
            }

            if (profile.Entitlements?.Any() == true)
            {
                result.AppendLine($"\nEntitlements:");
                foreach (var entitlement in profile.Entitlements)
                {
                    var valueStr = entitlement.Value switch
                    {
                        string s => s,
                        bool b => b.ToString().ToLower(),
                        Array arr => $"[{string.Join(", ", arr.Cast<object>())}]",
                        System.Collections.IDictionary dict => JsonSerializer.Serialize(dict),
                        _ => entitlement.Value?.ToString() ?? "null"
                    };
                    result.AppendLine($"  {entitlement.Key}: {valueStr}");
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error parsing provisioning profile: {ex.Message}";
        }
    }
}
