using System.ComponentModel;
using System.Text;
using AppleAppStoreConnect;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
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
}
