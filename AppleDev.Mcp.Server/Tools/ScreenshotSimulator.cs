using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Take a screenshot of a simulator")]
    public static async Task<string> ScreenshotSimulator(
        [Description("Simulator UDID or name")] string target,
        [Description("Output file path for the screenshot")] string outputPath)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            return "Error: Target simulator is required.";
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return "Error: Output path is required.";
        }

        try
        {
            var simctl = new SimCtl();
            var success = await simctl.RecordScreenshotAsync(target, new FileInfo(outputPath));

            if (success)
            {
                return $"Successfully captured screenshot to: {outputPath}";
            }
            else
            {
                return $"Failed to capture screenshot from simulator: {target}";
            }
        }
        catch (Exception ex)
        {
            return $"Error taking screenshot: {ex.Message}";
        }
    }
}
