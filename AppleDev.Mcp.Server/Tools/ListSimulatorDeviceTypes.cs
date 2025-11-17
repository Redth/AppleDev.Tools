using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List available simulator device types")]
    public static async Task<string> ListSimulatorDeviceTypes()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        try
        {
            var simctl = new SimCtl();
            var deviceTypes = await simctl.GetSimulatorGroupsAsync();

            if (!deviceTypes.Any())
            {
                return "No device types found.";
            }

            var result = new StringBuilder("Simulator Device Types:\n\n");
            foreach (var dt in deviceTypes)
            {
                result.AppendLine($"Name: {dt.Name}");
                result.AppendLine($"  Identifier: {dt.Identifier}");
                result.AppendLine($"  Product Family: {dt.ProductFamily}");
                result.AppendLine();
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing device types: {ex.Message}";
        }
    }
}
