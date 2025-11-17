using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Delete a keychain")]
    public static async Task<string> DeleteKeychain(
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
            var result = await keychain.DeleteKeychainAsync(name);

            if (result.Success)
            {
                return $"Successfully deleted keychain: {name}";
            }
            else
            {
                var error = !string.IsNullOrEmpty(result.StdErr) ? result.StdErr : "Unknown error";
                return $"Failed to delete keychain: {error}";
            }
        }
        catch (Exception ex)
        {
            return $"Error deleting keychain: {ex.Message}";
        }
    }
}
