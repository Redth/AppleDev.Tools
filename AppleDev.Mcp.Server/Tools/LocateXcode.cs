using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Find the active or best available Xcode installation")]
    public static async Task<string> LocateXcode(
        [Description("Find best available Xcode (highest version) instead of currently selected")] bool best = false)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        try
        {
            var xcode = new Xcode();

            if (best)
            {
                var bestXcode = await xcode.LocateBestAsync();
                
                if (bestXcode == null)
                {
                    return "No Xcode installation found.";
                }

                var result = new StringBuilder("Best Available Xcode:\n\n");
                result.AppendLine($"Path: {bestXcode.Path}");
                result.AppendLine($"Version: {bestXcode.Version}");
                result.AppendLine($"Selected: {(bestXcode.Selected ? "Yes" : "No")}");

                return result.ToString();
            }
            else
            {
                var selectedXcode = await xcode.LocateAsync();
                
                if (selectedXcode == null)
                {
                    return "No Xcode selected. Use 'xcode-select' to set one.";
                }

                return $"Selected Xcode Path: {selectedXcode.FullName}";
            }
        }
        catch (Exception ex)
        {
            return $"Error locating Xcode: {ex.Message}";
        }
    }
}
