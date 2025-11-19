using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Uninstall an app from a simulator")]
    public static async Task<string> UninstallSimulatorApp(
        [Description("Simulator UDID or name")] string target,
        [Description("Bundle identifier (e.g., com.company.appname)")] string bundleIdentifier)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            return "Error: Target simulator is required.";
        }

        if (string.IsNullOrWhiteSpace(bundleIdentifier))
        {
            return "Error: Bundle identifier is required.";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.UninstallAppAsync(target, bundleIdentifier);

            if (success)
            {
                return $"Successfully uninstalled app '{bundleIdentifier}' from simulator: {target}";
            }
            else
            {
                return $"Failed to uninstall app from simulator: {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error uninstalling app: {ex.Message}";
        }
    }
}
