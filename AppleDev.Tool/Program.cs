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

			sim.AddBranch("app", app =>
			{
				app.AddCommand<InstallSimulatorAppCommand>("install")
					.WithData(data)
					.WithDescription("Installs an app to a simulator")
					.WithExample(new[] { "simulator", "app", "install", "\"My iPhone 16\"", "~/MyApp.app" })
					.WithExample(new[] { "simulator", "app", "install", "booted", "~/build/MyApp.app" });

				app.AddCommand<UninstallSimulatorAppCommand>("uninstall")
					.WithData(data)
					.WithDescription("Uninstalls an app from a simulator")
					.WithExample(new[] { "simulator", "app", "uninstall", "\"My iPhone 16\"", "com.mycompany.myapp" })
					.WithExample(new[] { "simulator", "app", "uninstall", "booted", "com.mycompany.myapp" });

				app.AddCommand<LaunchSimulatorAppCommand>("launch")
					.WithData(data)
					.WithDescription("Launches an installed app on a simulator")
					.WithExample(new[] { "simulator", "app", "launch", "\"My iPhone 16\"", "com.mycompany.myapp" })
					.WithExample(new[] { "simulator", "app", "launch", "booted", "com.mycompany.myapp" });

				app.AddCommand<TerminateSimulatorAppCommand>("terminate")
					.WithData(data)
					.WithDescription("Terminates a running app on a simulator")
					.WithExample(new[] { "simulator", "app", "terminate", "\"My iPhone 16\"", "com.mycompany.myapp" })
					.WithExample(new[] { "simulator", "app", "terminate", "booted", "com.mycompany.myapp" });

				app.AddCommand<ListSimulatorAppsCommand>("list")
					.WithData(data)
					.WithDescription("Lists all installed apps on a simulator")
					.WithExample(new[] { "simulator", "app", "list", "booted" })
					.WithExample(new[] { "simulator", "app", "list", "\"My iPhone 16\"", "--format", "json" })
					.WithExample(new[] { "simulator", "app", "list", "booted", "--verbose" });
			});

			sim.AddCommand<OpenSimulatorCommand>("open")
				.WithData(data)
				.WithDescription("Opens Simulator.app to a specific simulator or the default")
				.WithExample(new[] { "simulator", "open" })
				.WithExample(new[] { "simulator", "open", "ABCD1234-1234-1234-1234-123456789ABC" });

			sim.AddCommand<DeviceTypesSimulatorCommand>("device-types")
				.WithData(data)
				.WithDescription("Lists available simulator device types")
				.WithExample(new[] { "simulator", "device-types" })
				.WithExample(new[] { "simulator", "device-types", "--format", "json" });

			sim.AddCommand<OpenUrlSimulatorCommand>("open-url")
				.WithData(data)
				.WithDescription("Opens a URL in a simulator (useful for deep linking)")
				.WithExample(new[] { "simulator", "open-url", "\"My iPhone 16\"", "\"myapp://deeplink/action\"" })
				.WithExample(new[] { "simulator", "open-url", "booted", "\"https://example.com\"" });

			sim.AddCommand<LogsSimulatorCommand>("logs")
				.WithData(data)
				.WithDescription("Retrieves simulator logs with optional filtering")
				.WithExample(new[] { "simulator", "logs", "booted" })
				.WithExample(new[] { "simulator", "logs", "booted", "--predicate", "\"eventMessage contains 'error'\"" })
				.WithExample(new[] { "simulator", "logs", "\"My iPhone 16\"", "--start", "\"2025-10-30 10:00:00\"" })
				.WithExample(new[] { "simulator", "logs", "booted", "--format", "json" });

			// Location commands
			sim.AddBranch("location", loc =>
			{
				loc.AddCommand<SetLocationSimulatorCommand>("set")
					.WithData(data)
					.WithDescription("Sets a simulated GPS location on the simulator")
					.WithExample(new[] { "simulator", "location", "set", "booted", "37.7749", "-122.4194" })
					.WithExample(new[] { "simulator", "location", "set", "\"My iPhone 16\"", "51.5074", "-0.1278" });

				loc.AddCommand<ClearLocationSimulatorCommand>("clear")
					.WithData(data)
					.WithDescription("Clears the simulated GPS location")
					.WithExample(new[] { "simulator", "location", "clear", "booted" });
			});

			// Push notification command
			sim.AddCommand<PushSimulatorCommand>("push")
				.WithData(data)
				.WithDescription("Sends a simulated push notification to an app")
				.WithExample(new[] { "simulator", "push", "booted", "com.example.app", "--title", "\"Hello\"", "--body", "\"World\"" })
				.WithExample(new[] { "simulator", "push", "booted", "com.example.app", "--payload", "notification.json" })
				.WithExample(new[] { "simulator", "push", "booted", "com.example.app", "--json", "\"{\\\"aps\\\":{\\\"alert\\\":\\\"Hello\\\"}}\"" });

			// Privacy commands
			sim.AddBranch("privacy", priv =>
			{
				priv.AddCommand<GrantPrivacySimulatorCommand>("grant")
					.WithData(data)
					.WithDescription("Grants a privacy permission to an app")
					.WithExample(new[] { "simulator", "privacy", "grant", "booted", "photos", "com.example.app" })
					.WithExample(new[] { "simulator", "privacy", "grant", "booted", "camera", "com.example.app" })
					.WithExample(new[] { "simulator", "privacy", "grant", "booted", "location", "com.example.app" });

				priv.AddCommand<RevokePrivacySimulatorCommand>("revoke")
					.WithData(data)
					.WithDescription("Revokes a privacy permission from an app")
					.WithExample(new[] { "simulator", "privacy", "revoke", "booted", "photos", "com.example.app" });

				priv.AddCommand<ResetPrivacySimulatorCommand>("reset")
					.WithData(data)
					.WithDescription("Resets a privacy permission (prompts user again)")
					.WithExample(new[] { "simulator", "privacy", "reset", "booted", "all" })
					.WithExample(new[] { "simulator", "privacy", "reset", "booted", "photos", "com.example.app" });
			});

			// Keychain command
			sim.AddCommand<ResetKeychainSimulatorCommand>("reset-keychain")
				.WithData(data)
				.WithDescription("Resets the simulator's keychain")
				.WithExample(new[] { "simulator", "reset-keychain", "booted" });

			// Status bar commands
			sim.AddBranch("status-bar", sb =>
			{
				sb.AddCommand<SetStatusBarSimulatorCommand>("set")
					.WithData(data)
					.WithDescription("Overrides status bar values (time, battery, signal, etc.)")
					.WithExample(new[] { "simulator", "status-bar", "set", "booted", "--time", "9:41" })
					.WithExample(new[] { "simulator", "status-bar", "set", "booted", "--battery-level", "100", "--battery-state", "charged" })
					.WithExample(new[] { "simulator", "status-bar", "set", "booted", "--cellular-bars", "4", "--wifi-bars", "3" });

				sb.AddCommand<ClearStatusBarSimulatorCommand>("clear")
					.WithData(data)
					.WithDescription("Clears status bar overrides")
					.WithExample(new[] { "simulator", "status-bar", "clear", "booted" });
			});

			// Clipboard commands
			sim.AddBranch("clipboard", cb =>
			{
				cb.AddCommand<CopyClipboardSimulatorCommand>("copy")
					.WithData(data)
					.WithDescription("Copies text to the simulator's clipboard")
					.WithExample(new[] { "simulator", "clipboard", "copy", "booted", "\"Hello, World!\"" });

				cb.AddCommand<PasteClipboardSimulatorCommand>("paste")
					.WithData(data)
					.WithDescription("Gets text from the simulator's clipboard")
					.WithExample(new[] { "simulator", "clipboard", "paste", "booted" })
					.WithExample(new[] { "simulator", "clipboard", "paste", "booted", "--raw" });
			});

			// IDB-specific commands (require idb_companion)
			sim.AddBranch("idb", idb =>
			{
				idb.AddCommand<IdbMemoryWarningCommand>("memory-warning")
					.WithData(data)
					.WithDescription("Simulates a memory warning (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "memory-warning", "<udid>" });

				idb.AddCommand<IdbTapCommand>("tap")
					.WithData(data)
					.WithDescription("Taps at screen coordinates (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "tap", "<udid>", "100", "200" })
					.WithExample(new[] { "simulator", "idb", "tap", "<udid>", "100", "200", "--duration", "500" });

				idb.AddCommand<IdbSwipeCommand>("swipe")
					.WithData(data)
					.WithDescription("Performs a swipe gesture (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "swipe", "<udid>", "100", "500", "100", "100" })
					.WithExample(new[] { "simulator", "idb", "swipe", "<udid>", "100", "500", "100", "100", "--duration", "300" });

				idb.AddCommand<IdbKeyCommand>("key")
					.WithData(data)
					.WithDescription("Sends keyboard input (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "key", "<udid>", "--text", "Hello" })
					.WithExample(new[] { "simulator", "idb", "key", "<udid>", "--key-code", "40" });

				idb.AddCommand<IdbButtonCommand>("button")
					.WithData(data)
					.WithDescription("Presses a hardware button (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "button", "<udid>", "home" })
					.WithExample(new[] { "simulator", "idb", "button", "<udid>", "lock" });

				idb.AddCommand<IdbFocusCommand>("focus")
					.WithData(data)
					.WithDescription("Focuses the simulator window (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "focus", "<udid>" });

				idb.AddCommand<IdbKeyboardCommand>("keyboard")
					.WithData(data)
					.WithDescription("Toggles hardware keyboard (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "keyboard", "<udid>", "--enable" })
					.WithExample(new[] { "simulator", "idb", "keyboard", "<udid>", "--disable" });

				idb.AddCommand<IdbAccessibilityCommand>("accessibility")
					.WithData(data)
					.WithDescription("Gets accessibility info (requires idb_companion)")
					.WithExample(new[] { "simulator", "idb", "accessibility", "<udid>" })
					.WithExample(new[] { "simulator", "idb", "accessibility", "<udid>", "-x", "100", "-y", "200" });

				// File operations
				idb.AddBranch("files", files =>
				{
					files.AddCommand<IdbFilesListCommand>("list")
						.WithData(data)
						.WithDescription("Lists files in app container")
						.WithExample(new[] { "simulator", "idb", "files", "list", "<udid>", "com.example.app" })
						.WithExample(new[] { "simulator", "idb", "files", "list", "<udid>", "com.example.app", "/Documents" });

					files.AddCommand<IdbFilesPushCommand>("push")
						.WithData(data)
						.WithDescription("Pushes file to app container")
						.WithExample(new[] { "simulator", "idb", "files", "push", "<udid>", "com.example.app", "local.txt", "/Documents/remote.txt" });

					files.AddCommand<IdbFilesPullCommand>("pull")
						.WithData(data)
						.WithDescription("Pulls file from app container")
						.WithExample(new[] { "simulator", "idb", "files", "pull", "<udid>", "com.example.app", "/Documents/data.db", "local.db" });

					files.AddCommand<IdbFilesMkdirCommand>("mkdir")
						.WithData(data)
						.WithDescription("Creates directory in app container")
						.WithExample(new[] { "simulator", "idb", "files", "mkdir", "<udid>", "com.example.app", "/Documents/cache" });

					files.AddCommand<IdbFilesRemoveCommand>("rm")
						.WithData(data)
						.WithDescription("Removes file/directory from app container")
						.WithExample(new[] { "simulator", "idb", "files", "rm", "<udid>", "com.example.app", "/Documents/old.txt" });
				});

				// Crash log operations
				idb.AddBranch("crashes", crashes =>
				{
					crashes.AddCommand<IdbCrashesListCommand>("list")
						.WithData(data)
						.WithDescription("Lists crash logs")
						.WithExample(new[] { "simulator", "idb", "crashes", "list", "<udid>" })
						.WithExample(new[] { "simulator", "idb", "crashes", "list", "<udid>", "--query", "MyApp" });

					crashes.AddCommand<IdbCrashesGetCommand>("get")
						.WithData(data)
						.WithDescription("Gets crash log content")
						.WithExample(new[] { "simulator", "idb", "crashes", "get", "<udid>", "crash_name.ips" })
						.WithExample(new[] { "simulator", "idb", "crashes", "get", "<udid>", "crash_name.ips", "-o", "crash.txt" });

					crashes.AddCommand<IdbCrashesDeleteCommand>("delete")
						.WithData(data)
						.WithDescription("Deletes crash logs")
						.WithExample(new[] { "simulator", "idb", "crashes", "delete", "<udid>", "--all" })
						.WithExample(new[] { "simulator", "idb", "crashes", "delete", "<udid>", "--query", "MyApp" });
				});

				// Test operations
				idb.AddBranch("tests", tests =>
				{
					tests.AddCommand<IdbTestsListCommand>("list")
						.WithData(data)
						.WithDescription("Lists tests in xctest bundle")
						.WithExample(new[] { "simulator", "idb", "tests", "list", "<udid>", "MyTests.xctest" });

					tests.AddCommand<IdbTestsRunCommand>("run")
						.WithData(data)
						.WithDescription("Runs tests from xctest bundle")
						.WithExample(new[] { "simulator", "idb", "tests", "run", "<udid>", "MyTests.xctest" })
						.WithExample(new[] { "simulator", "idb", "tests", "run", "<udid>", "MyTests.xctest", "-f", "MyTestClass/testMethod" });
				});

				// Settings operations
				idb.AddBranch("settings", settings =>
				{
					settings.AddCommand<IdbSettingsListCommand>("list")
						.WithData(data)
						.WithDescription("Lists device settings")
						.WithExample(new[] { "simulator", "idb", "settings", "list", "<udid>" });

					settings.AddCommand<IdbSettingsGetCommand>("get")
						.WithData(data)
						.WithDescription("Gets a device setting")
						.WithExample(new[] { "simulator", "idb", "settings", "get", "<udid>", "locale" });

					settings.AddCommand<IdbSettingsSetCommand>("set")
						.WithData(data)
						.WithDescription("Sets a device setting")
						.WithExample(new[] { "simulator", "idb", "settings", "set", "<udid>", "locale", "en_US" });
				});
			});
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

		config.AddBranch("xcode", xcode =>
		{
			xcode.AddCommand<ListXcodeCommand>("list")
				.WithData(data)
				.WithDescription("Lists all installed Xcode versions")
				.WithExample(new[] { "xcode", "list" })
				.WithExample(new[] { "xcode", "list", "--format", "json" });

			xcode.AddCommand<LocateXcodeCommand>("locate")
				.WithData(data)
				.WithDescription("Finds the currently selected or best available Xcode installation")
				.WithExample(new[] { "xcode", "locate" })
				.WithExample(new[] { "xcode", "locate", "--best" })
				.WithExample(new[] { "xcode", "locate", "--best", "--format", "json" });
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
		
		provisioning.AddCommand<CreateProvisioningProfileCommand>("create")
			.WithData(data)
			.WithDescription("Creates a new provisioning profile in App Store Connect")
			.WithExample(new[] { "provisioning", "create", "--name", "My App Dev Profile", "--type", "IOS_APP_DEVELOPMENT", "--bundle-id", "BNDL123", "--certificates", "CERT456", "--devices", "DEV789,DEV012" })
			.WithExample(new[] { "provisioning", "create", "--name", "My App Store Profile", "--type", "IOS_APP_STORE", "--bundle-id", "BNDL123", "--certificates", "CERT456" })
			.WithExample(new[] { "provisioning", "create", "--name", "My Dev Profile", "--type", "IOS_APP_DEVELOPMENT", "--bundle-id", "BNDL123", "--certificates", "CERT456,CERT789", "--all-devices", "--download" });
		
		provisioning.AddCommand<DeleteProvisioningProfileCommand>("delete")
			.WithData(data)
			.WithDescription("Deletes a provisioning profile from App Store Connect")
			.WithExample(new[] { "provisioning", "delete", "PROF123" });
	});
	
	config.AddBranch("devices", devices =>
	{
		devices.AddCommand<ListAscDevicesCommand>("list")
			.WithData(data)
			.WithDescription("Lists devices registered in App Store Connect")
			.WithExample(new[] { "devices", "list" })
			.WithExample(new[] { "devices", "list", "--platform", "IOS" })
			.WithExample(new[] { "devices", "list", "--status", "ENABLED", "--format", "json" });
		
		devices.AddCommand<RegisterDeviceCommand>("register")
			.WithData(data)
			.WithDescription("Registers a new device in App Store Connect")
			.WithExample(new[] { "devices", "register", "John's iPhone", "00008030-001234567890001E", "IOS" })
			.WithExample(new[] { "devices", "register", "Test Mac", "12345678-1234-1234-1234-123456789012", "MAC_OS" });
		
		devices.AddCommand<UpdateDeviceCommand>("update")
			.WithData(data)
			.WithDescription("Updates a device registration in App Store Connect")
			.WithExample(new[] { "devices", "update", "ABC123", "--name", "John's New iPhone" })
			.WithExample(new[] { "devices", "update", "ABC123", "--status", "DISABLED" });
	});
	
	config.AddBranch("bundleids", bundleids =>
	{
		bundleids.AddCommand<ListBundleIdsCommand>("list")
			.WithData(data)
			.WithDescription("Lists App Store Connect bundle identifiers (app IDs)")
			.WithExample(new[] { "bundleids", "list" })
			.WithExample(new[] { "bundleids", "list", "--format", "json" })
			.WithExample(new[] { "bundleids", "list", "--verbose" });
		
		bundleids.AddCommand<CreateBundleIdCommand>("create")
			.WithData(data)
			.WithDescription("Creates a new bundle ID in App Store Connect")
			.WithExample(new[] { "bundleids", "create", "My App", "com.mycompany.myapp", "IOS" })
			.WithExample(new[] { "bundleids", "create", "My App", "com.mycompany.myapp", "UNIVERSAL" })
			.WithExample(new[] { "bundleids", "create", "Wildcard App", "com.mycompany.*", "IOS" })
			.WithExample(new[] { "bundleids", "create", "Mac App", "com.mycompany.macapp", "MAC_OS", "--seed-id", "ABCD1234" });
		
		bundleids.AddCommand<UpdateBundleIdCommand>("update")
			.WithData(data)
			.WithDescription("Updates an existing bundle ID")
			.WithExample(new[] { "bundleids", "update", "ABC123", "--name", "My Renamed App" });
		
		bundleids.AddCommand<DeleteBundleIdCommand>("delete")
			.WithData(data)
			.WithDescription("Deletes a bundle ID from App Store Connect")
			.WithExample(new[] { "bundleids", "delete", "ABC123" });
	});

	config.AddBranch("capabilities", capabilities =>
	{
		capabilities.AddCommand<ListBundleIdCapabilitiesCommand>("list")
			.WithData(data)
			.WithDescription("Lists capabilities enabled for a bundle ID")
			.WithExample(new[] { "capabilities", "list", "BUNDLEID123" })
			.WithExample(new[] { "capabilities", "list", "BUNDLEID123", "--format", "json" });
		
		capabilities.AddCommand<EnableBundleIdCapabilityCommand>("enable")
			.WithData(data)
			.WithDescription("Enables a capability for a bundle ID")
			.WithExample(new[] { "capabilities", "enable", "BUNDLEID123", "PUSH_NOTIFICATIONS" })
			.WithExample(new[] { "capabilities", "enable", "BUNDLEID123", "ICLOUD" })
			.WithExample(new[] { "capabilities", "enable", "BUNDLEID123", "GAME_CENTER" });
		
		capabilities.AddCommand<DisableBundleIdCapabilityCommand>("disable")
			.WithData(data)
			.WithDescription("Disables (removes) a capability from a bundle ID")
			.WithExample(new[] { "capabilities", "disable", "CAPID456" });
	});

	config.AddBranch("certificate", certificates =>
	{
		certificates.AddCommand<ListCertificatesCommand>("list")
			.WithData(data)
			.WithDescription("Lists certificates from App Store Connect")
			.WithExample(new[] { "certificate", "list" })
			.WithExample(new[] { "certificate", "list", "--type", "IOS_DISTRIBUTION" })
			.WithExample(new[] { "certificate", "list", "--format", "json" });
		
		certificates.AddCommand<CreateCertificateCommand>("create")
			.WithData(data)
			.WithDescription("Creates a new signing certificate via App Store Connect")
			.WithExample(new[] { "certificate", "create" })
			.WithExample(new[] { "certificate", "create", "--output", "~/certificates/" });
		
		certificates.AddCommand<RevokeCertificateCommand>("revoke")
			.WithData(data)
			.WithDescription("Revokes a certificate in App Store Connect")
			.WithExample(new[] { "certificate", "revoke", "ABC123" });
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

	config.AddCommand<AppInfoCommand>("app-info")
		.WithData(data)
		.WithDescription("Reads and displays Info.plist metadata from an app bundle")
		.WithExample(new[] { "app-info", "~/MyApp.app" })
		.WithExample(new[] { "app-info", "~/MyApp.app", "--format", "json" })
		.WithExample(new[] { "app-info", "~/MyApp.app", "--verbose" });

	config.AddCommand<UploadAppCommand>("upload")
		.WithData(data)
		.WithDescription("Uploads an app to App Store Connect / TestFlight")
		.WithExample(new[] { "upload", "~/MyApp.ipa", "--type", "ios" })
		.WithExample(new[] { "upload", "~/MyApp.app", "--type", "macos" })
		.WithExample(new[] { "upload", "~/MyApp.ipa", "--type", "ios", "--key-id", "ABC123DEF4", "--issuer-id", "12345678-1234-1234-1234-123456789012" });

	config.AddCommand<ValidateAppCommand>("validate")
		.WithData(data)
		.WithDescription("Validates an app before uploading to App Store Connect")
		.WithExample(new[] { "validate", "~/MyApp.ipa", "--type", "ios" })
		.WithExample(new[] { "validate", "~/MyApp.app", "--type", "macos" })
		.WithExample(new[] { "validate", "~/MyApp.ipa", "--type", "ios", "--key-id", "ABC123DEF4", "--issuer-id", "12345678-1234-1234-1234-123456789012" });
});


try
{
	app.Run(args);
}
catch (Exception ex)
{
	AnsiConsole.WriteException(ex);
}