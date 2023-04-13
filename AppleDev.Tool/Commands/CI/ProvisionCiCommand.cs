using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ProvisionCiCommand : AsyncCommand<ProvisionCiCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ProvisionCiCommandSettings settings)
	{
		var data = context.GetData();

		AnsiConsole.Write(new Rule("Provisioning..."));

		var certificateData = settings.GetBytesFromFileOrEnvironmentOrBase64String(settings.Certificate);

		if (certificateData is not null)
		{
			var keychain = new Keychain();

			if (!string.IsNullOrEmpty(settings.Keychain) && settings.Keychain != Keychain.DefaultKeychain)
			{
				AnsiConsole.Write($"Creating Keychain {settings.Keychain}...");
				var createResult = await keychain.CreateKeychainAsync(settings.Keychain, settings.KeychainPassword, data.CancellationToken).ConfigureAwait(false);

				if (!createResult.Success)
				{
					AnsiConsole.WriteLine();
					createResult.OutputFailure("Creating Keychain Failed");
					return 1;
				}
					
				AnsiConsole.WriteLine($" Done.");
			}	

			AnsiConsole.Write($"Importing Certificate into {settings.Keychain} (AllowAnyAppRead: {settings.AllowAnyAppRead})...");

			var tmpFile = Path.GetTempFileName();
			File.WriteAllBytes(tmpFile, certificateData);

			var importResult = await keychain.ImportPkcs12Async(tmpFile, settings.CertificatePassphrase, settings.Keychain, settings.AllowAnyAppRead, data.CancellationToken).ConfigureAwait(false);

			if (!importResult.Success)
			{
				AnsiConsole.WriteLine();
				importResult.OutputFailure("Import Certificate Failed");
				return 1;
			}

			AnsiConsole.WriteLine($" Done.");
		}


		if (settings.BundleIdentifiers.Length > 0)
		{
			AnsiConsole.Write($"Installing Provisioning Profiles for: {string.Join(", ", settings.BundleIdentifiers)}...");

			var profileResults = new List<ProvisioningProfile>();

			var apiPrivateKey = settings.GetStringFromFileOrEnvironmentOrString(settings.ApiPrivateKey);

			var appStoreConnect = new AppStoreConnectClient(new AppStoreConnectConfiguration(settings.ApiKeyId, settings.ApiIssuerId, apiPrivateKey!));

			var profiles = await appStoreConnect.ListProfilesAsync(
				filterProfileState: new[] { ProfileState.ACTIVE },
				filterProfileType: settings.ProfileTypes.Length > 0 ? settings.ProfileTypes : null,
				include: "bundleId",
				limit: 200)
				.ConfigureAwait(false);

			foreach (var profile in profiles.Data)
			{
				// Get the Bundle ID for the profile
				var profileBundleId = profiles.IncludedBundleIds?.FirstOrDefault()?.Attributes;

				if (settings.BundleIdentifiers.Length > 0)
				{
					if (profileBundleId is not null && settings.BundleIdentifiers.Any(b => profileBundleId?.IdentifierMatches(b) ?? false))
						profileResults.Add(new ProvisioningProfile(profile.Attributes, profileBundleId));
				}
				else
				{
					profileResults.Add(new ProvisioningProfile(profile.Attributes, profileBundleId));
				}
			}


			foreach (var p in profileResults)
			{
				AnsiConsole.WriteLine($"Installing profile: {p.Profile.Name} ({p.Profile.Uuid}) - {p.Profile.ProfileTypeValue} -  {p.Profile.PlatformValue} - BundleID: {p.BundleId?.Identifier}");
			}


			if (profileResults.Count > 0)
			{
				// Install profiles
				await appStoreConnect.InstallProfilesAsync(profileResults.Select(p => p.Profile), settings.ProfilePath).ConfigureAwait(false);
			}

			AnsiConsole.WriteLine($"Done - {profileResults.Count} Provisioning Profiles Installed.");

		}

		return this.ExitCode();
	}
}
public class ProvisionCiCommandSettings : CommandSettings
{
	[Description("Base64 encoded certificate data, certificate filename, or environment variable name with base 64 encoded certificate data")]
	[CommandOption("--certificate")]
	public string Certificate { get; set; } = string.Empty;

	[Description("Certificate's passphrase")]
	[CommandOption("--certificate-passphrase <passphrase>")]
	public string CertificatePassphrase { get; set; } = string.Empty;

	[Description("Keychain name to import into")]
	[DefaultValue("login.keychain-db")]
	[CommandOption("--keychain <keychain>")]
	public string Keychain { get; set; } = AppleDev.Keychain.DefaultKeychain;

	[Description("Keychain password")]
	[CommandOption("--keychain-password <password>")]
	public string KeychainPassword { get; set; } = string.Empty;

	[Description("Allows any app read permission")]
	[CommandOption("--keychain-allow-any-app-read")]
	public bool AllowAnyAppRead { get; set; }


	[Description("App bundle identifier(s) to match provisioning profiles for")]
	[CommandOption("--bundle-identifier <BUNDLE_IDENTIFIER>")]
	public string[] BundleIdentifiers { get; set; } = new string[0];

	[Description("Provisioning profile type(s) to match / download")]
	[CommandOption("--profile-type <PROFILE_TYPE>")]
	public ProfileType[] ProfileTypes { get; set; } = new ProfileType[0];


	[Description("Override default directory to save downloaded profiles to")]
	[CommandOption("--profile-path <directory>")]
	public DirectoryInfo? ProfilePath { get; set; }


	[Description("AppStoreConnect Key ID")]
	[CommandOption("--api-key-id <key_id>")]
	public string ApiKeyId { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_KEY_ID") ?? string.Empty;

	[Description("AppStoreConnect Issuer ID")]
	[CommandOption("--api-issuer-id <issuer_id>")]
	public string ApiIssuerId { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_ISSUER_ID") ?? string.Empty;

	[Description("AppStoreConnect Private Key Filename or Base64 string or Environment variable with Base64 string")]
	[CommandOption("--api-private-key <private_key>")]
	public string ApiPrivateKey { get; set; }
		= Environment.GetEnvironmentVariable("APP_STORE_CONNECT_PRIVATE_KEY") ?? string.Empty;
}