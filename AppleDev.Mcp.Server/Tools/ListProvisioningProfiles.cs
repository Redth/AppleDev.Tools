using System.ComponentModel;
using System.Text;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List provisioning profiles with optional filtering")]
    public static async Task<string> ListProvisioningProfiles(
        [Description("Filter by profile type (IOS_APP_DEVELOPMENT, IOS_APP_STORE, IOS_APP_ADHOC)")] string? profileType = null,
        [Description("Filter by profile state (ACTIVE, INVALID)")] string? profileState = null,
        [Description("Filter by profile name")] string? name = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var filterType = profileType != null ? new[] { Enum.Parse<ProfileType>(profileType, true) } : null;
        var filterState = profileState != null ? new[] { Enum.Parse<ProfileState>(profileState, true) } : null;
        var filterName = name != null ? new[] { name } : null;

        var response = await client.ListProfilesAsync(
            filterProfileType: filterType,
            filterProfileState: filterState,
            filterName: filterName);

        if (response?.Data == null || !response.Data.Any())
        {
            return "No provisioning profiles found matching the criteria.";
        }

        var result = new StringBuilder("Provisioning Profiles:\n\n");
        foreach (var profile in response.Data)
        {
            result.AppendLine($"ID: {profile.Id}");
            result.AppendLine($"  Name: {profile.Attributes.Name}");
            result.AppendLine($"  Type: {profile.Attributes.ProfileType}");
            result.AppendLine($"  State: {profile.Attributes.ProfileState}");
            if (!string.IsNullOrEmpty(profile.Attributes.Uuid))
                result.AppendLine($"  UUID: {profile.Attributes.Uuid}");
            if (!string.IsNullOrEmpty(profile.Attributes.PlatformValue))
                result.AppendLine($"  Platform: {profile.Attributes.Platform}");
            if (profile.Attributes.CreatedDate.HasValue)
                result.AppendLine($"  Created: {profile.Attributes.CreatedDate}");
            if (profile.Attributes.ExpirationDate.HasValue)
                result.AppendLine($"  Expiration: {profile.Attributes.ExpirationDate}");
            result.AppendLine();
        }

        return result.ToString();
    }
}
