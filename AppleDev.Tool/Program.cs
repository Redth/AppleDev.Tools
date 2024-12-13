using AppleDev.Tool.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
	if (!cts.IsCancellationRequested)
		cts.Cancel();
};

var data = new CommandContextData
{
	CancellationToken = cts.Token
};

var app = new CommandApp();
app.Configure(config =>
{
	if (OperatingSystem.IsMacOS())
	{
		config
		.AddBranch("simulator", sim =>
		{
			sim.AddCommand<ListSimulatorsCommand>("list")
				.WithData(data)
				.WithDescription("Lists Simulators")
				.WithExample(new[] { "simulator", "list" })
				.WithExample(new[] { "simulator", "list", "--available" })
				.WithExample(new[] { "simulator", "list", "--booted" });

			sim.AddCommand<BootSimulatorCommand>("boot")
				.WithData(data)
				.WithDescription("Boots a simulator")
				.WithExample(new[] { "simulator", "boot", "1234-1112-12242-12322-1111232" })
				.WithExample(new[] { "simulator", "boot", "--wait", "1234-1112-12242-12322-1111232" })
				.WithExample(new[] { "simulator", "boot", "--wait", "--timeout", "240", "1234-1112-12242-12322-1111232" });

			sim.AddCommand<ShutdownSimulatorCommand>("shutdown")
				.WithData(data)
				.WithDescription("Shuts down simulator(s)")
				.WithExample(new[] { "simulator", "shutdown", "1234-1112-12242-12322-1111232" })
				.WithExample(new[] { "simulator", "shutdown", "all" })
				.WithExample(new[] { "simulator", "shutdown", "Booted" });

			sim.AddCommand<EraseSimulatorCommand>("erase")
				.WithData(data)
				.WithDescription("Erases / resets simulator(s)")
				.WithExample(new[] { "simulator", "erase", "1234-1112-12242-12322-1111232" })
				.WithExample(new[] { "simulator", "erase", "all" })
				.WithExample(new[] { "simulator", "erase", "Booted" });

			sim.AddCommand<ScreenshotSimulatorCommand>("screenshot")
				.WithData(data)
				.WithDescription("Records a screenshot")
				.WithExample(new[] { "simulator", "screenshot", "1234-1112-12242-12322-1111232" })
				.WithExample(new[]
					{ "simulator", "screenshot", "--output", "~/screenshot.png", "1234-1112-12242-12322-1111232" });
		});

		config.AddBranch("device", devices =>
		{
			devices.AddCommand<ListDevicesCommand>("list")
				.WithData(data)
				.WithDescription("Lists devices")
				.WithExample(new[] { "device", "list" });
		});

		config.AddBranch("keychain", keychain =>
		{
			keychain.AddCommand<UnlockKeychainCommand>("unlock")
				.WithData(data)
				.WithDescription("Unlocks a keychain file")
				.WithExample(new[] { "keychain", "unlock", "--allow-any-app-read", "--keychain", "~/Library/Keychains/login.keychain-db", "--password", "temp1234" });

			keychain.AddCommand<ImportPkcs12KeychainCommand>("import")
				.WithData(data)
				.WithDescription("Imports a certificate into a keychain")
				.WithExample(new[] { "keychain", "import", "~/mycert.p12", "--keychain", "~/Library/Keychains/login.keychain-db" });

			keychain.AddCommand<CreateKeychainCommand>("create")
				.WithData(data)
				.WithDescription("Creates a new keychain")
				.WithExample(new[] { "keychain", "create", "--keychain", "~/Library/Keychains/ci-temp.keychain-db", "--password", "temp1234" });

			keychain.AddCommand<DeleteKeychainCommand>("delete")
				.WithData(data)
				.WithDescription("Deletes a keychain")
				.WithExample(new[] { "keychain", "delete", "--keychain", "~/Library/Keychains/ci-temp.keychain-db" });
			
			keychain.AddCommand<SetDefaultKeychainCommand>("default")
				.WithData(data)
				.WithDescription("Sets the default keychain")
				.WithExample(new[] { "keychain", "default", "--keychain", "~/Library/Keychains/ci-temp.keychain-db" });

		});
	}

	config.AddBranch("provisioning", provisioning =>
	{
		provisioning.AddCommand<ListProvisioningProfilesCommand>("list")
			.WithData(data)
			.WithDescription("List provisioning profiles and optionally downloads/installs them")
			.WithExample(new[] { "provisioning", "list" })
			.WithExample(new[] { "provisioning", "list", "--download" });
		
		provisioning.AddCommand<ListInstalledProvisioningProfilesCommand>("installed")
			.WithData(data)
			.WithDescription("List locally installed provisioning profiles")
			.WithExample(new[] { "provisioning", "installed" });
		
		provisioning.AddCommand<ParseProvisioningProfileCommand>("parse")
			.WithData(data)
			.WithDescription("Parse a given provisioning profile")
			.WithExample(new[] { "provisioning", "parse", "profile.mobileprovision" });
	});
	
	config.AddBranch("bundleids", bundleids =>
	{
		bundleids.AddCommand<ListBundleIdsCommand>("list")
			.WithData(data)
			.WithDescription("List bundle identifiers")
			.WithExample(new[] { "bundleids", "list" });
	});

	config.AddBranch("certificate", provisioning =>
	{
		provisioning.AddCommand<CreateCertificateCommand>("create")
			.WithData(data)
			.WithDescription("Create new certificate")
			.WithExample(new[] { "certificate", "create" });

	});

	config.AddBranch("ci", ci =>
	{
		ci.AddCommand<ProvisionCiCommand>("provision")
			.WithData(data)
			.WithDescription("Provisions a CI environment with certificates, keychain, provisioning profiles")
			.WithExample(new[] { "ci", "provision", "--certificate", "ENV_VAR_NAME_WITH_BASE64_CERT_DATA", "--bundle-identifier", "com.myapp.id" })
			.WithExample(new[] { "ci", "provision", "--certificate", "ENV_VAR_NAME_WITH_BASE64_CERT_DATA", "--bundle-identifier", "com.myapp.id", "--app-store-connect-key-id", "XYZ123ABC", "--app-store-connect-issuer-id", "aaaa-1111-bbbbb-2222-cccc", "--app-store-connect-private-key", "ENV_VAR_NAME_P8_FILE_CONTENTS" })
			.WithExample(new[] { "ci", "provision", "--keychain", "tmp.keychain", "--certificate", "CERT_BASE_64_STRING", "--bundle-identifier", "com.myapp.id" });

		ci.AddCommand<DeprovisionCiCommand>("deprovision")
			.WithData(data)
			.WithDescription("Deprovisions a CI environment previously provisioned with the provision command")
			.WithExample(new[] { "ci", "deprovision", "--keychain", "tmp.keychain" });

		ci.AddCommand<CreateSecretCommand>("secret")
			.WithData(data)
			.WithDescription("Creates a base64 encoded string suitable for CI secret storage from a private key (.p8) or certificate (.p12 or .pfx) file")
			.WithExample(new[] { "ci", "secret", "--from-certificate", "dev.p12" })
			.WithExample(new[] { "ci", "secret", "--from-private-key", "WXYZ1234.p8" });
		
		ci.AddCommand<Base64ToFileCommand>("base64-to-file")
			.WithData(data)
			.WithDescription("Writes a base64 encoded string value from direct input, or an environment variable name to a file")
			.WithExample(new[] { "ci", "base64-to-file", "--base64", "ENV_VAR_NAME", "--output-file", "dev.p12" })
			.WithExample(new[] { "ci", "base64-to-file", "--base64", "cmVkdGg=", "--output-file", "some.file" });
		
		ci.AddCommand<EnvironmentVariableToFileCommand>("env-to-file")
			.WithData(data)
			.WithDescription("Writes the value of an environment variable to a file")
			.WithExample(new[] { "ci", "env-to-file", "--env-var", "ENV_VAR_NAME", "--output-file", "apikey.p8" });
			
	});
});


try
{
	app.Run(args);
}
catch (Exception ex)
{
	AnsiConsole.WriteException(ex);
}