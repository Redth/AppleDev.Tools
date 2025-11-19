using System.ComponentModel;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
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
}
