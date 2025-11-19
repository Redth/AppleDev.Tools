using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
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
}
