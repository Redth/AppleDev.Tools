using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Create a new simulator")]
    public static async Task<string> CreateSimulator(
        [Description("Name for the new simulator")] string name,
        [Description("Device type (e.g., 'iPhone 15' or 'com.apple.CoreSimulator.SimDeviceType.iPhone-15')")] string deviceType,
        [Description("Runtime identifier (optional)")] string? runtime = null)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return "Error: Simulator name is required.";
        }

        if (string.IsNullOrWhiteSpace(deviceType))
        {
            return "Error: Device type is required.";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.CreateAsync(name, deviceType, runtime);

            if (success)
            {
                return $"Successfully created simulator '{name}' with device type '{deviceType}'.";
            }
            else
            {
                return $"Failed to create simulator '{name}'.";
            }
        }
        catch (Exception ex)
        {
            return $"Error creating simulator: {ex.Message}";
        }
    }
}
