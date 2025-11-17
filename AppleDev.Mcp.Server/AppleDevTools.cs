using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for managing Apple development resources including devices, bundle IDs, certificates, and provisioning profiles.
/// </summary>
[McpServerToolType]
public static partial class AppleDevTools
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
}
