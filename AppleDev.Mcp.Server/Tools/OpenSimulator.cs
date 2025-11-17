using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Open Simulator.app")]
    public static async Task<string> OpenSimulator(
        [Description("Optional simulator UDID to open directly")] string? udid = null)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.OpenSimulatorAppAsync(udid);

            if (success)
            {
                if (!string.IsNullOrEmpty(udid))
                {
                    return $"Successfully opened Simulator.app with UDID: {udid}";
                }
                else
                {
                    return "Successfully opened Simulator.app";
                }
            }
            else
            {
                return "Failed to open Simulator.app";
            }
        }
        catch (FileNotFoundException ex)
        {
            return $"Simulator.app not found: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error opening Simulator.app: {ex.Message}";
        }
    }
}
