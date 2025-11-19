using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List connected physical devices and simulators using xcdevice")]
    public static async Task<string> ListDevicesAndSimulators(
        [Description("Timeout in seconds to search for network devices (default: 5)")] int timeout = 5,
        [Description("Show only physical devices, exclude simulators (default: false)")] bool devicesOnly = false)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        try
        {
            var xcdevice = new XCDevice();
            var devices = await xcdevice.GetDevicesAsync(
                cancellationToken: default,
                timeout: TimeSpan.FromSeconds(timeout));

            if (devicesOnly)
            {
                devices = devices.Where(d => !d.Simulator).ToList();
            }

            if (!devices.Any())
            {
                return devicesOnly 
                    ? "No physical devices found." 
                    : "No devices or simulators found.";
            }

            var result = new StringBuilder();
            result.AppendLine(devicesOnly ? "Physical Devices:\n" : "Devices and Simulators:\n");

            foreach (var device in devices)
            {
                result.AppendLine($"Name: {device.Name}");
                result.AppendLine($"  Identifier: {device.Identifier}");
                result.AppendLine($"  Platform: {device.Platform}");
                result.AppendLine($"  OS Version: {device.OperatingSystemVersion}");
                result.AppendLine($"  Model Name: {device.ModelName}");
                result.AppendLine($"  Model Code: {device.ModelCode}");
                result.AppendLine($"  Architecture: {device.Architecture}");
                result.AppendLine($"  Interface: {device.Interface}");
                result.AppendLine($"  Available: {device.Available}");
                result.AppendLine($"  Simulator: {device.Simulator}");
                result.AppendLine();
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing devices: {ex.Message}";
        }
    }
}
