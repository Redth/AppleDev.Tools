using System.ComponentModel;
using AppleDev;
using ModelContextProtocol.Server;

public static partial class AppleDevTools
{
    [McpServerTool, Description("Import a PKCS12 certificate into a keychain")]
    public static async Task<string> ImportPkcs12Keychain(
        [Description("Path to PKCS12/PFX certificate file")] string certificatePath,
        [Description("Certificate passphrase")] string passphrase,
        [Description("Keychain name (default: login.keychain-db)")] string? keychain = null,
        [Description("Allow any app to read the certificate (default: false)")] bool allowAnyAppRead = false)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return "Error: This tool is only available on macOS.";
        }

        if (string.IsNullOrWhiteSpace(certificatePath))
        {
            return "Error: Certificate path is required.";
        }

        if (!File.Exists(certificatePath))
        {
            return $"Error: Certificate file not found at path: {certificatePath}";
        }

        if (string.IsNullOrWhiteSpace(passphrase))
        {
            return "Error: Certificate passphrase is required.";
        }

        try
        {
            var keychainObj = new Keychain();
            var keychainName = keychain ?? Keychain.DefaultKeychain;
            var result = await keychainObj.ImportPkcs12Async(
                certificatePath, 
                passphrase, 
                keychainName, 
                allowAnyAppRead);

            if (result.Success)
            {
                return $"Successfully imported certificate into keychain: {keychainName}\n" +
                       $"Certificate: {Path.GetFileName(certificatePath)}";
            }
            else
            {
                var error = !string.IsNullOrEmpty(result.StdErr) ? result.StdErr : "Unknown error";
                return $"Failed to import certificate: {error}";
            }
        }
        catch (Exception ex)
        {
            return $"Error importing certificate: {ex.Message}";
        }
    }
}
