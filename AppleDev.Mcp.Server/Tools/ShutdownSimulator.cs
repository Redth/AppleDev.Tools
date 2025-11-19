using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Shutdown a simulator")]
    public static async Task<string> ShutdownSimulator(
        [Description("Simulator UDID, name, or 'all' for all simulators")] string target = "all")
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.ShutdownAsync(target);

            if (success)
            {
                return $"Successfully shut down simulator(s): {target}";
            }
            else
            {
                return $"Failed to shut down simulator(s): {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error shutting down simulator: {ex.Message}";
        }
    }
}
