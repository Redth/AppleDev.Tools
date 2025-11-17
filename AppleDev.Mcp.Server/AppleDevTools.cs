using System.ComponentModel;
using System.Text;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for managing Apple development resources including devices, bundle IDs, certificates, and provisioning profiles.
/// </summary>
[McpServerToolType]
public static class AppleDevTools
{
    /// <summary>
    /// Gets an App Store Connect client using credentials from environment variables.
    /// Supports multiple credential sets using a naming convention:
    /// - Default: APP_STORE_CONNECT_KEY_ID, APP_STORE_CONNECT_ISSUER_ID, APP_STORE_CONNECT_PRIVATE_KEY
    /// - Named: APP_STORE_CONNECT_KEY_ID_ACME, APP_STORE_CONNECT_ISSUER_ID_ACME, APP_STORE_CONNECT_PRIVATE_KEY_ACME
    /// </summary>
    /// <param name="keyName">Optional key name suffix (e.g., "ACME", "TAILWIND"). If null, uses default credentials.</param>
    private static AppStoreConnectClient GetClient(string? keyName = null)
    {
        var suffix = string.IsNullOrEmpty(keyName) ? "" : $"_{keyName.ToUpperInvariant()}";

        var keyId = Environment.GetEnvironmentVariable($"APP_STORE_CONNECT_KEY_ID{suffix}")
            ?? throw new InvalidOperationException($"APP_STORE_CONNECT_KEY_ID{suffix} environment variable not set");

        var issuerId = Environment.GetEnvironmentVariable($"APP_STORE_CONNECT_ISSUER_ID{suffix}")
            ?? throw new InvalidOperationException($"APP_STORE_CONNECT_ISSUER_ID{suffix} environment variable not set");

        var privateKey = Environment.GetEnvironmentVariable($"APP_STORE_CONNECT_PRIVATE_KEY{suffix}")
            ?? throw new InvalidOperationException($"APP_STORE_CONNECT_PRIVATE_KEY{suffix} environment variable not set");

        var config = new AppStoreConnectConfiguration(keyId, issuerId, privateKey);
        return new AppStoreConnectClient(config);
    }

    // ========== DEVICE MANAGEMENT ==========

    [McpServerTool, Description("List registered Apple devices with optional filtering by name, platform, status, or UDID")]
    public static async Task<string> ListDevices(
        [Description("Filter by device name (partial match)")] string? name = null,
        [Description("Filter by device platform (IOS, MAC_OS, etc.)")] string? platform = null,
        [Description("Filter by device status (ENABLED, DISABLED)")] string? status = null,
        [Description("Filter by device UDID (exact match)")] string? udid = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var filterName = name != null ? new[] { name } : null;
        var filterPlatform = platform != null ? new[] { Enum.Parse<Platform>(platform, true) } : null;
        var filterStatus = status != null ? new[] { Enum.Parse<DeviceStatus>(status, true) } : null;
        var filterUdid = udid != null ? new[] { udid } : null;

        var response = await client.ListDevicesAsync(
            filterName: filterName,
            filterPlatform: filterPlatform,
            filterStatus: filterStatus,
            filterUdid: filterUdid);

        if (response?.Data == null || !response.Data.Any())
        {
            return "No devices found matching the criteria.";
        }

        var result = new StringBuilder("Registered Devices:\n\n");
        foreach (var device in response.Data)
        {
            result.AppendLine($"ID: {device.Id}");
            result.AppendLine($"  Name: {device.Attributes.Name}");
            result.AppendLine($"  UDID: {device.Attributes.Udid}");
            result.AppendLine($"  Platform: {device.Attributes.Platform}");
            result.AppendLine($"  Status: {device.Attributes.Status}");
            if (!string.IsNullOrEmpty(device.Attributes.Model))
                result.AppendLine($"  Model: {device.Attributes.Model}");
            if (!string.IsNullOrEmpty(device.Attributes.DeviceClass))
                result.AppendLine($"  Device Class: {device.Attributes.DeviceClass}");
            result.AppendLine($"  Added: {device.Attributes.AddedDate}");
            result.AppendLine();
        }

        return result.ToString();
    }

    [McpServerTool, Description("Register a new Apple device for development")]
    public static async Task<string> RegisterDevice(
        [Description("Device name (e.g., 'John's iPhone 15')")] string name,
        [Description("Device UDID (40-character hex string)")] string udid,
        [Description("Device platform (IOS, MAC_OS, UNIVERSAL)")] string platform,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var attributes = new DeviceAttributes
        {
            Name = name,
            Udid = udid,
            Platform = Enum.Parse<Platform>(platform, true)
        };

        var response = await client.RegisterDeviceAsync(attributes);

        if (response?.Data == null)
        {
            return "Failed to register device.";
        }

        return $"Device registered successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"UDID: {response.Data.Attributes.Udid}\n" +
               $"Platform: {response.Data.Attributes.Platform}\n" +
               $"Status: {response.Data.Attributes.Status}";
    }

    [McpServerTool, Description("Modify an existing device's name or status")]
    public static async Task<string> ModifyDevice(
        [Description("Device ID to modify")] string deviceId,
        [Description("New device name (optional)")] string? name = null,
        [Description("New device status: ENABLED or DISABLED (optional)")] string? status = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var attributes = new DeviceAttributes();

        if (name != null)
            attributes.Name = name;

        if (status != null)
            attributes.StatusValue = status;

        var response = await client.ModifyDeviceAsync(deviceId, attributes);

        if (response?.Data == null)
        {
            return "Failed to modify device.";
        }

        return $"Device modified successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"Status: {response.Data.Attributes.Status}";
    }

    // ========== BUNDLE ID MANAGEMENT ==========

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

    [McpServerTool, Description("Create a new bundle ID")]
    public static async Task<string> CreateBundleId(
        [Description("Display name for the bundle ID")] string name,
        [Description("Bundle identifier (e.g., 'com.example.app')")] string identifier,
        [Description("Platform (IOS, MAC_OS, UNIVERSAL)")] string platform,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var attributes = new BundleIdAttributes
        {
            Name = name,
            Identifier = identifier,
            Platform = Enum.Parse<Platform>(platform, true)
        };

        var response = await client.CreateBundleIdAsync(attributes);

        if (response?.Data == null)
        {
            return "Failed to create bundle ID.";
        }

        return $"Bundle ID created successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"Identifier: {response.Data.Attributes.Identifier}\n" +
               $"Platform: {response.Data.Attributes.Platform}";
    }

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

    // ========== CERTIFICATE MANAGEMENT ==========

    [McpServerTool, Description("List certificates with optional filtering")]
    public static async Task<string> ListCertificates(
        [Description("Filter by certificate type (e.g., IOS_DEVELOPMENT, IOS_DISTRIBUTION)")] string? certificateType = null,
        [Description("Filter by display name")] string? displayName = null,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var filterType = certificateType != null ? new[] { Enum.Parse<CertificateType>(certificateType, true) } : null;
        var filterDisplayName = displayName != null ? new[] { displayName } : null;

        var response = await client.ListCertificatesAsync(
            filterCertificateType: filterType,
            filterDisplayName: filterDisplayName);

        if (response?.Data == null || !response.Data.Any())
        {
            return "No certificates found matching the criteria.";
        }

        var result = new StringBuilder("Certificates:\n\n");
        foreach (var cert in response.Data)
        {
            result.AppendLine($"ID: {cert.Id}");
            result.AppendLine($"  Name: {cert.Attributes.Name}");
            result.AppendLine($"  Type: {cert.Attributes.CertificateType}");
            if (!string.IsNullOrEmpty(cert.Attributes.SerialNumber))
                result.AppendLine($"  Serial Number: {cert.Attributes.SerialNumber}");
            if (!string.IsNullOrEmpty(cert.Attributes.PlatformValue))
                result.AppendLine($"  Platform: {cert.Attributes.Platform}");
            if (!string.IsNullOrEmpty(cert.Attributes.DisplayName))
                result.AppendLine($"  Display Name: {cert.Attributes.DisplayName}");
            result.AppendLine();
        }

        return result.ToString();
    }

    [McpServerTool, Description("Create a new certificate (auto-generates CSR)")]
    public static async Task<string> CreateCertificate(
        [Description("Certificate type (IOS_DEVELOPMENT, IOS_DISTRIBUTION, MAC_APP_DEVELOPMENT, DEVELOPER_ID_APPLICATION)")] string certificateType,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var certType = Enum.Parse<CertificateType>(certificateType, true);
        var response = await client.CreateCertificateAsync(certificateType: certType);

        if (response?.Data == null)
        {
            return "Failed to create certificate.";
        }

        return $"Certificate created successfully!\n" +
               $"ID: {response.Data.Id}\n" +
               $"Name: {response.Data.Attributes.Name}\n" +
               $"Type: {response.Data.Attributes.CertificateType}\n" +
               $"Serial Number: {response.Data.Attributes.SerialNumber}\n" +
               $"Certificate content length: {response.Data.Attributes.CertificateContent?.Length ?? 0} chars";
    }

    [McpServerTool, Description("Revoke (delete) a certificate")]
    public static async Task<string> RevokeCertificate(
        [Description("Certificate ID to revoke")] string certificateId,
        [Description("App Store Connect key name (for using alternate credential sets)")] string? keyName = null)
    {
        var client = GetClient(keyName);

        var success = await client.RevokeCertificateAsync(certificateId);

        return success
            ? $"Certificate {certificateId} revoked successfully!"
            : $"Failed to revoke certificate {certificateId}.";
    }

    // ========== PROVISIONING PROFILE MANAGEMENT ==========

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
