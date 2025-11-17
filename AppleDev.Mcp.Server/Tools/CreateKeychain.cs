using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Create a new keychain")]
    public static async Task<string> CreateKeychain(
        [Description("Keychain name")] string name,
        [Description("Keychain password")] string password)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return "Error: Keychain name is required.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Error: Keychain password is required.";
        }

        try
        {
            var keychain = new Keychain();
            var result = await keychain.CreateKeychainAsync(name, password);

            if (result.Success)
            {
                var keychainPath = keychain.Locate(name).FullName;
                return $"Successfully created keychain: {name}\nPath: {keychainPath}";
            }
            else
            {
                var error = !string.IsNullOrEmpty(result.StdErr) ? result.StdErr : "Unknown error";
                return $"Failed to create keychain: {error}";
            }
        }
        catch (Exception ex)
        {
            return $"Error creating keychain: {ex.Message}";
        }
    }
}
