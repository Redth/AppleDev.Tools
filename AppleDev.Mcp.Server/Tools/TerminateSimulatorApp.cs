using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Terminate a running app on a simulator")]
    public static async Task<string> TerminateSimulatorApp(
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
            var success = await simctl.TerminateAppAsync(target, bundleIdentifier);

            if (success)
            {
                return $"Successfully terminated app '{bundleIdentifier}' on simulator: {target}";
            }
            else
            {
                return $"Failed to terminate app on simulator: {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error terminating app: {ex.Message}";
        }
    }
}
