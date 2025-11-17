using System.ComponentModel;
using System.Text;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Get information about an .app bundle")]
    public static async Task<string> GetAppInfo(
        [Description("Path to .app bundle")] string appPath)
    {
        await Task.CompletedTask; // Make it properly async

        if (string.IsNullOrWhiteSpace(appPath))
        {
            return "Error: App path is required.";
        }

        if (!Directory.Exists(appPath) && !File.Exists(appPath))
        {
            return $"Error: App bundle not found at path: {appPath}";
        }

        try
        {
            var reader = new AppBundleReader(appPath);
            var infoPlist = reader.ReadInfoPlist();

            var result = new StringBuilder("App Bundle Information:\n\n");
            result.AppendLine($"Bundle Identifier: {infoPlist.CFBundleIdentifier ?? "Unknown"}");
            result.AppendLine($"Display Name: {infoPlist.CFBundleDisplayName ?? infoPlist.CFBundleName ?? "Unknown"}");
            result.AppendLine($"Bundle Version: {infoPlist.CFBundleVersion ?? "Unknown"}");
            result.AppendLine($"Short Version String: {infoPlist.CFBundleShortVersionString ?? "Unknown"}");
            result.AppendLine($"Platform Name: {infoPlist.DTPlatformName ?? "Unknown"}");
            result.AppendLine($"Platform Version: {infoPlist.DTPlatformVersion ?? "Unknown"}");
            result.AppendLine($"SDK Name: {infoPlist.DTSDKName ?? "Unknown"}");
            result.AppendLine($"Info.plist Path: {reader.InfoPlistFile ?? "Unknown"}");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error reading app bundle: {ex.Message}";
        }
    }
}
