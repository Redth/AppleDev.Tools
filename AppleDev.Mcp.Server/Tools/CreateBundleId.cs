using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
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
}
