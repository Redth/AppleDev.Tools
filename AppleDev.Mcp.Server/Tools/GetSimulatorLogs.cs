using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Retrieve simulator logs")]
    public static async Task<string> GetSimulatorLogs(
        [Description("Simulator UDID, name, or 'booted' for all booted simulators")] string target,
        [Description("Filter events using a predicate (optional)")] string? predicate = null,
        [Description("Maximum number of log lines to return (default: 100)")] int maxLines = 100)
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
            // Get logs from recent time to avoid overwhelming output
            var startTime = DateTimeOffset.Now.AddMinutes(-5);
            var logs = await simctl.GetLogsPlainAsync(target, predicate, startTime);

            if (!logs.Any())
            {
                return $"No logs found for simulator: {target}";
            }

            // Limit output to avoid overwhelming the response
            var limitedLogs = logs.TakeLast(maxLines).ToList();
            
            var result = new StringBuilder($"Simulator logs for '{target}' (last {limitedLogs.Count} lines):\n\n");
            foreach (var log in limitedLogs)
            {
                result.AppendLine(log);
            }

            if (logs.Count > maxLines)
            {
                result.AppendLine($"\n... ({logs.Count - maxLines} more lines not shown)");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving simulator logs: {ex.Message}";
        }
    }
}
