using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Install an app on a simulator")]
    public static async Task<string> InstallSimulatorApp(
        [Description("Simulator UDID or name")] string target,
        [Description("Path to .app bundle")] string appPath)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            return "Error: Target simulator is required.";
        }

        if (string.IsNullOrWhiteSpace(appPath))
        {
            return "Error: App path is required.";
        }

        if (!Directory.Exists(appPath))
        {
            return $"Error: App bundle not found at path: {appPath}";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.InstallAppAsync(target, new DirectoryInfo(appPath));

            if (success)
            {
                return $"Successfully installed app '{appPath}' on simulator: {target}";
            }
            else
            {
                return $"Failed to install app on simulator: {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error installing app: {ex.Message}";
        }
    }
}
