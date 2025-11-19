using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List installed apps on a simulator")]
    public static async Task<string> ListSimulatorApps(
        [Description("Simulator UDID or 'booted' for all booted simulators")] string target)
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
            var apps = await simctl.GetAppsAsync(target);

            if (!apps.Any())
            {
                return $"No apps found on simulator: {target}";
            }

            var result = new StringBuilder($"Apps installed on simulator '{target}':\n\n");
            foreach (var app in apps)
            {
                result.AppendLine($"Bundle ID: {app.CFBundleIdentifier}");
                result.AppendLine($"  Name: {app.CFBundleDisplayName ?? app.CFBundleName}");
                result.AppendLine($"  Version: {app.CFBundleVersion}");
                result.AppendLine($"  Path: {app.Path}");
                result.AppendLine();
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing apps: {ex.Message}";
        }
    }
}
