using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("List available simulators")]
    public static async Task<string> ListSimulators(
        [Description("Show only booted simulators")] bool booted = false,
        [Description("Show only available simulators")] bool available = false,
        [Description("Show only unavailable simulators")] bool unavailable = false,
        [Description("Filter by simulator name")] string? name = null,
        [Description("Filter by UDID")] string? udid = null,
        [Description("Filter by runtime (e.g., 'iOS 18.3')")] string? runtime = null,
        [Description("Filter by device type (e.g., 'iPhone 16 Pro')")] string? deviceType = null)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (available && unavailable)
        {
            return "Error: Specify either available or unavailable, not both.";
        }

        try
        {
            var simctl = new SimCtl();
            var sims = await simctl.GetSimulatorsAsync(availableOnly: false);

            if (available)
                sims = sims.Where(sim => sim.IsAvailable).ToList();
            if (unavailable)
                sims = sims.Where(sim => !sim.IsAvailable).ToList();
            if (booted)
                sims = sims.Where(sim => sim.IsBooted).ToList();
            if (!string.IsNullOrEmpty(name))
                sims = sims.Where(sim => sim.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!string.IsNullOrEmpty(udid))
                sims = sims.Where(sim => sim.Udid?.Equals(udid, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!string.IsNullOrEmpty(runtime))
                sims = sims.Where(sim => sim.Runtime?.Name?.Equals(runtime, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!string.IsNullOrEmpty(deviceType))
                sims = sims.Where(sim => sim.DeviceType?.Name?.Equals(deviceType, StringComparison.OrdinalIgnoreCase) == true).ToList();

            if (!sims.Any())
            {
                return "No simulators found matching the criteria.";
            }

            var result = new StringBuilder("Simulators:\n\n");
            foreach (var sim in sims)
            {
                result.AppendLine($"Name: {sim.Name}");
                result.AppendLine($"  UDID: {sim.Udid}");
                result.AppendLine($"  State: {sim.State}");
                result.AppendLine($"  Available: {sim.IsAvailable}");
                result.AppendLine($"  Device Type: {sim.DeviceType?.Name}");
                result.AppendLine($"  Runtime: {sim.Runtime?.Name} ({sim.Runtime?.Version})");
                result.AppendLine();
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing simulators: {ex.Message}";
        }
    }
}
