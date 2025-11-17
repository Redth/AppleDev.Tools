using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Unlock a keychain")]
    public static async Task<string> UnlockKeychain(
        [Description("Keychain password")] string password,
        [Description("Keychain name or path (default: login.keychain-db)")] string? name = null)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Error: Keychain password is required.";
        }

        try
        {
            var keychain = new Keychain();
            var keychainName = name ?? Keychain.DefaultKeychain;
            var result = await keychain.UnlockKeychainAsync(password, keychainName);

            if (result.Success)
            {
                return $"Successfully unlocked keychain: {keychainName}";
            }
            else
            {
                var error = !string.IsNullOrEmpty(result.StdErr) ? result.StdErr : "Unknown error";
                return $"Failed to unlock keychain: {error}";
            }
        }
        catch (Exception ex)
        {
            return $"Error unlocking keychain: {ex.Message}";
        }
    }
}
