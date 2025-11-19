using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Erase all content from a simulator")]
    public static async Task<string> EraseSimulator(
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
            var success = await simctl.EraseAsync(target);

            if (success)
            {
                return $"Successfully erased simulator: {target}";
            }
            else
            {
                return $"Failed to erase simulator: {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error erasing simulator: {ex.Message}";
        }
    }
}
