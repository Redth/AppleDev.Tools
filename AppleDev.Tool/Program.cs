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
	config.SetApplicationName("apple");
	
	config.AddExample(new[] { "simulator", "list" });
	config.AddExample(new[] { "simulator", "create", "\"My iPhone 15\"", "--device-type", "\"iPhone 15\"" });
	config.AddExample(new[] { "device", "list" });
	config.AddExample(new[] { "provisioning", "list", "--download" });
	config.AddExample(new[] { "ci", "provision", "--certificate", "IOS_CERT_BASE64", "--bundle-identifier", "com.myapp.id" });

	if (OperatingSystem.IsMacOS())
	{
		config.AddBranch("simulator", sim =>
		{
			sim.AddCommand<ListSimulatorsCommand>("list")
				.WithData(data)
				.WithDescription("Lists iOS, watchOS, tvOS, and visionOS simulators with filtering options")
				.WithExample(new[] { "simulator", "list" })
				.WithExample(new[] { "simulator", "list", "--available" })
				.WithExample(new[] { "simulator", "list", "--booted" })
				.WithExample(new[] { "simulator", "list", "--unavailable" })
				.WithExample(new[] { "simulator", "list", "--name", "\"My iPhone 15\"" })
				.WithExample(new[] { "simulator", "list", "--device-type", "\"iPhone 16 Pro\"" })
				.WithExample(new[] { "simulator", "list", "--runtime", "\"iOS 18.3\"" })
				.WithExample(new[] { "simulator", "list", "--product-family", "\"iPhone\"" })
				.WithExample(new[] { "simulator", "list", "--format", "json" })
				.WithExample(new[] { "simulator", "list", "--verbose" });

			sim.AddCommand<CreateSimulatorCommand>("create")
				.WithData(data)
				.WithDescription("Creates a new simulator with specified device type and runtime")
				.WithExample(new[] { "simulator", "create", "\"My iPhone 15\"", "--device-type", "\"iPhone 15\"" })
				.WithExample(new[] { "simulator", "create", "\"My iPhone 15 Pro\"", "--device-type", "\"iPhone 15 Pro\"", "--runtime", "\"iOS 17.0\"" })
				.WithExample(new[] { "simulator", "create", "\"Test Device\"", "--device-type", "\"com.apple.CoreSimulator.SimDeviceType.iPhone-15\"" })
				.WithExample(new[] { "simulator", "create", "\"Apple Watch Series 9\"", "--device-type", "\"Apple Watch Series 9 (45mm)\"", "--runtime", "\"watchOS 10.0\"" })
				.WithExample(new[] { "simulator", "create", "\"Apple TV 4K\"", "--device-type", "\"Apple TV 4K (3rd generation)\"" });

			sim.AddCommand<BootSimulatorCommand>("boot")
				.WithData(data)
				.WithDescription("Boots a simulator and optionally waits for it to be ready")
				.WithExample(new[] { "simulator", "boot", "ABCD1234-1234-1234-1234-123456789ABC" })
				.WithExample(new[] { "simulator", "boot", "\"My iPhone 15\"" })
				.WithExample(new[] { "simulator", "boot", "--wait", "\"My iPhone 15\"" })
				.WithExample(new[] { "simulator", "boot", "--wait", "--timeout", "180", "ABCD1234-1234-1234-1234-123456789ABC" });

			sim.AddCommand<ShutdownSimulatorCommand>("shutdown")
				.WithData(data)
				.WithDescription("Shuts down running simulators (accepts UDID, name, or status)")
				.WithExample(new[] { "simulator", "shutdown", "ABCD1234-1234-1234-1234-123456789ABC" })
				.WithExample(new[] { "simulator", "shutdown", "\"My iPhone 15\"" })
				.WithExample(new[] { "simulator", "shutdown", "all" })
				.WithExample(new[] { "simulator", "shutdown", "booted" });

			sim.AddCommand<EraseSimulatorCommand>("erase")
				.WithData(data)
				.WithDescription("Erases simulator content and settings (factory reset)")
				.WithExample(new[] { "simulator", "erase", "ABCD1234-1234-1234-1234-123456789ABC" })
				.WithExample(new[] { "simulator", "erase", "\"My iPhone 15\"" })
				.WithExample(new[] { "simulator", "erase", "all" })
				.WithExample(new[] { "simulator", "erase", "booted" });

			sim.AddCommand<DeleteSimulatorCommand>("delete")
				.WithData(data)
				.WithDescription("Permanently deletes simulators (cannot be undone)")
				.WithExample(new[] { "simulator", "delete", "ABCD1234-1234-1234-1234-123456789ABC" })
				.WithExample(new[] { "simulator", "delete", "\"My Old iPhone\"" })
				.WithExample(new[] { "simulator", "delete", "unavailable" })
				.WithExample(new[] { "simulator", "delete", "all" });

			sim.AddCommand<ScreenshotSimulatorCommand>("screenshot")
				.WithData(data)
				.WithDescription("Captures a screenshot from a running simulator")
				.WithExample(new[] { "simulator", "screenshot", "ABCD1234-1234-1234-1234-123456789ABC" })
				.WithExample(new[] { "simulator", "screenshot", "\"My iPhone 15\"" })
				.WithExample(new[] { "simulator", "screenshot", "--output", "~/Desktop/screenshot.png", "ABCD1234-1234-1234-1234-123456789ABC" })
				.WithExample(new[] { "simulator", "screenshot", "--output", "~/screenshots/ios-app-$(date +%Y%m%d-%H%M%S).png", "\"My iPhone 15\"" });
		});

		config.AddBranch("device", devices =>
		{
			devices.AddCommand<ListDevicesCommand>("list")
				.WithData(data)
				.WithDescription("Lists connected iOS, watchOS, tvOS, and visionOS devices")
				.WithExample(new[] { "device", "list" })
				.WithExample(new[] { "device", "list", "--format", "json" })
				.WithExample(new[] { "device", "list", "--verbose" });
		});

		config.AddBranch("keychain", keychain =>
		{
			keychain.AddCommand<UnlockKeychainCommand>("unlock")
				.WithData(data)
				.WithDescription("Unlocks a keychain file for automated access")
				.WithExample(new[] { "keychain", "unlock", "--keychain", "~/Library/Keychains/login.keychain-db", "--password", "mypassword" })
				.WithExample(new[] { "keychain", "unlock", "--allow-any-app-read", "--keychain", "~/Library/Keychains/ci-temp.keychain-db", "--password", "temp1234" });

			keychain.AddCommand<ImportPkcs12KeychainCommand>("import")
				.WithData(data)
				.WithDescription("Imports a PKCS#12 certificate (.p12/.pfx) into a keychain")
				.WithExample(new[] { "keychain", "import", "~/certificates/ios-distribution.p12", "--keychain", "~/Library/Keychains/login.keychain-db" })
				.WithExample(new[] { "keychain", "import", "~/certs/dev-cert.pfx", "--keychain", "~/Library/Keychains/ci-temp.keychain-db", "--password", "certpassword" });

			keychain.AddCommand<CreateKeychainCommand>("create")
				.WithData(data)
				.WithDescription("Creates a new keychain file with specified password")
				.WithExample(new[] { "keychain", "create", "--keychain", "~/Library/Keychains/ci-temp.keychain-db", "--password", "temp1234" })
				.WithExample(new[] { "keychain", "create", "--keychain", "~/Library/Keychains/build-$(date +%Y%m%d).keychain-db", "--password", "buildpass" });

			keychain.AddCommand<DeleteKeychainCommand>("delete")
				.WithData(data)
				.WithDescription("Deletes a keychain file (use with caution)")
				.WithExample(new[] { "keychain", "delete", "--keychain", "~/Library/Keychains/ci-temp.keychain-db" })
				.WithExample(new[] { "keychain", "delete", "--keychain", "~/Library/Keychains/old-build.keychain-db" });
			
			keychain.AddCommand<SetDefaultKeychainCommand>("default")
				.WithData(data)
				.WithDescription("Sets the default keychain for the current user")
				.WithExample(new[] { "keychain", "default", "--keychain", "~/Library/Keychains/login.keychain-db" })
				.WithExample(new[] { "keychain", "default", "--keychain", "~/Library/Keychains/ci-temp.keychain-db" });

		});
	}

	config.AddBranch("provisioning", provisioning =>
	{
		provisioning.AddCommand<ListProvisioningProfilesCommand>("list")
			.WithData(data)
			.WithDescription("Lists App Store Connect provisioning profiles with download option")
			.WithExample(new[] { "provisioning", "list" })
			.WithExample(new[] { "provisioning", "list", "--download" })
			.WithExample(new[] { "provisioning", "list", "--format", "json" });
		
		provisioning.AddCommand<ListInstalledProvisioningProfilesCommand>("installed")
			.WithData(data)
			.WithDescription("Lists locally installed provisioning profiles on this Mac")
			.WithExample(new[] { "provisioning", "installed" })
			.WithExample(new[] { "provisioning", "installed", "--format", "json" })
			.WithExample(new[] { "provisioning", "installed", "--verbose" });
		
		provisioning.AddCommand<ParseProvisioningProfileCommand>("parse")
			.WithData(data)
			.WithDescription("Parses and displays information from a .mobileprovision file")
			.WithExample(new[] { "provisioning", "parse", "MyApp_Development.mobileprovision" })
			.WithExample(new[] { "provisioning", "parse", "~/Downloads/MyApp_AppStore.mobileprovision", "--format", "json" });
	});
	
	config.AddBranch("bundleids", bundleids =>
	{
		bundleids.AddCommand<ListBundleIdsCommand>("list")
			.WithData(data)
			.WithDescription("Lists App Store Connect bundle identifiers (app IDs)")
			.WithExample(new[] { "bundleids", "list" })
			.WithExample(new[] { "bundleids", "list", "--format", "json" })
			.WithExample(new[] { "bundleids", "list", "--verbose" });
	});

	config.AddBranch("certificate", certificates =>
	{
		certificates.AddCommand<CreateCertificateCommand>("create")
			.WithData(data)
			.WithDescription("Creates a new signing certificate via App Store Connect")
			.WithExample(new[] { "certificate", "create" })
			.WithExample(new[] { "certificate", "create", "--output", "~/certificates/" });
	});

	config.AddBranch("ci", ci =>
	{
		ci.AddCommand<ProvisionCiCommand>("provision")
			.WithData(data)
			.WithDescription("Sets up CI environment with certificates, keychain, and provisioning profiles")
			.WithExample(new[] { "ci", "provision", "--certificate", "IOS_DISTRIBUTION_CERT_BASE64", "--bundle-identifier", "com.mycompany.myapp" })
			.WithExample(new[] { "ci", "provision", "--certificate", "IOS_CERT_BASE64", "--bundle-identifier", "com.myapp.id", "--app-store-connect-key-id", "ABC123DEF4", "--app-store-connect-issuer-id", "12345678-1234-1234-1234-123456789012", "--app-store-connect-private-key", "ASC_PRIVATE_KEY_BASE64" })
			.WithExample(new[] { "ci", "provision", "--keychain", "ci-build.keychain", "--certificate", "CERT_BASE64", "--bundle-identifier", "com.example.app" });

		ci.AddCommand<DeprovisionCiCommand>("deprovision")
			.WithData(data)
			.WithDescription("Cleans up CI environment (removes temporary keychain and certificates)")
			.WithExample(new[] { "ci", "deprovision", "--keychain", "ci-build.keychain" })
			.WithExample(new[] { "ci", "deprovision" });

		ci.AddCommand<CreateSecretCommand>("secret")
			.WithData(data)
			.WithDescription("Converts certificates/keys to base64 for secure CI storage")
			.WithExample(new[] { "ci", "secret", "--from-certificate", "ios-distribution.p12" })
			.WithExample(new[] { "ci", "secret", "--from-certificate", "~/certificates/development.pfx" })
			.WithExample(new[] { "ci", "secret", "--from-private-key", "AuthKey_ABC123DEF4.p8" });
		
		ci.AddCommand<Base64ToFileCommand>("base64-to-file")
			.WithData(data)
			.WithDescription("Decodes base64 data from environment variable or string to file")
			.WithExample(new[] { "ci", "base64-to-file", "--base64", "IOS_CERT_BASE64", "--output-file", "distribution.p12" })
			.WithExample(new[] { "ci", "base64-to-file", "--base64", "aGVsbG8gd29ybGQ=", "--output-file", "decoded.txt" });
		
		ci.AddCommand<EnvironmentVariableToFileCommand>("env-to-file")
			.WithData(data)
			.WithDescription("Writes environment variable value directly to file (no base64 decoding)")
			.WithExample(new[] { "ci", "env-to-file", "--env-var", "APP_STORE_CONNECT_API_KEY", "--output-file", "AuthKey_ABC123DEF4.p8" })
			.WithExample(new[] { "ci", "env-to-file", "--env-var", "BUILD_CONFIG", "--output-file", "config.json" });
			
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