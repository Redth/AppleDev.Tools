using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Set the default keychain")]
    public static async Task<string> SetDefaultKeychain(
        [Description("Keychain name or path")] string name)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return "Error: Keychain name is required.";
        }

        try
        {
            var keychain = new Keychain();
            var result = await keychain.SetDefaultKeychainAsync(name);

            if (result.Success)
            {
                return $"Successfully set default keychain to: {name}";
            }
            else
            {
                var error = !string.IsNullOrEmpty(result.StdErr) ? result.StdErr : "Unknown error";
                return $"Failed to set default keychain: {error}";
            }
        }
        catch (Exception ex)
        {
            return $"Error setting default keychain: {ex.Message}";
        }
    }
}
