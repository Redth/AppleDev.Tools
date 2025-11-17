using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Boot a simulator")]
    public static async Task<string> BootSimulator(
        [Description("Simulator UDID or name")] string target)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            return "Error: Target simulator is required.";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.BootAsync(target);

            if (success)
            {
                return $"Successfully booted simulator: {target}";
            }
            else
            {
                return $"Failed to boot simulator: {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error booting simulator: {ex.Message}";
        }
    }
}
