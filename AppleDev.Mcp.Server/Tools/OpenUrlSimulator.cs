using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Open a URL in a simulator")]
    public static async Task<string> OpenUrlSimulator(
        [Description("Simulator UDID or name")] string target,
        [Description("URL to open")] string url)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            return "Error: Target simulator is required.";
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return "Error: URL is required.";
        }

        try
        {
            var uri = new Uri(url);
            var simctl = new SimCtl();
            var success = await simctl.OpenUrlAsync(target, uri);

            if (success)
            {
                return $"Successfully opened URL '{url}' on simulator: {target}";
            }
            else
            {
                return $"Failed to open URL on simulator: {target}";
            }
        }
        catch (UriFormatException ex)
        {
            return $"Error: Invalid URL format: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error opening URL: {ex.Message}";
        }
    }
}
