using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Delete a simulator")]
    public static async Task<string> DeleteSimulator(
        [Description("Simulator UDID, name, 'all', or status (e.g., 'unavailable')")] string target)
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
            var success = await simctl.DeleteAsync(target);

            if (success)
            {
                return $"Successfully deleted simulator(s): {target}";
            }
            else
            {
                return $"Failed to delete simulator(s): {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error deleting simulator: {ex.Message}";
        }
    }
}
