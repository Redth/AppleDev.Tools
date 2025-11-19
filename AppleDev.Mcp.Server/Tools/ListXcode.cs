using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List all installed Xcode versions")]
    public static async Task<string> ListXcode()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        try
        {
            var xcode = new Xcode();
            var xcodeInstalls = await xcode.LocateAllAsync();

            if (xcodeInstalls == null || !xcodeInstalls.Any())
            {
                return "No Xcode installations found.";
            }

            var result = new StringBuilder("Xcode Installations:\n\n");
            foreach (var install in xcodeInstalls)
            {
                result.AppendLine($"Path: {install.Path}");
                result.AppendLine($"  Version: {install.Version}");
                result.AppendLine($"  Selected: {(install.Selected ? "Yes" : "No")}");
                result.AppendLine();
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing Xcode installations: {ex.Message}";
        }
    }
}
