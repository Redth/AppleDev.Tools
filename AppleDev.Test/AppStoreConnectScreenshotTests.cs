using AppleAppStoreConnect;

namespace AppleDev.Test;

public class AppStoreConnectScreenshotTests
{
	static AppStoreConnectScreenshotTests()
	{
		// Load .env file if it exists in the project root
		DotEnvLoader.Load();
	}

	public AppStoreConnectScreenshotTests()
	{
		var keyId = Environment.GetEnvironmentVariable("APP_STORE_CONNECT_KEY_ID");
		var issuerId = Environment.GetEnvironmentVariable("APP_STORE_CONNECT_ISSUER_ID");
		var privateKey = Environment.GetEnvironmentVariable("APP_STORE_CONNECT_PRIVATE_KEY");

		// Skip tests if credentials are not configured
		if (string.IsNullOrWhiteSpace(keyId) ||
			string.IsNullOrWhiteSpace(issuerId) ||
			string.IsNullOrWhiteSpace(privateKey))
		{
			HasCredentials = false;
			return;
		}

		HasCredentials = true;
		Client = new AppStoreConnectClient(
			new AppStoreConnectConfiguration(keyId, issuerId, privateKey));

		// Get test bundle ID from environment (optional)
		TestBundleId = Environment.GetEnvironmentVariable("TEST_BUNDLE_ID");
	}

	readonly AppStoreConnectClient? Client;
	readonly bool HasCredentials;
	readonly string? TestBundleId;

	// ===== APP TESTS =====

	[SkippableFact]
	public async Task ListApps()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var apps = await Client!.ListAppsAsync(limit: 10);
		Assert.NotNull(apps);
		Assert.NotNull(apps.Data);
		// Apps may be empty for new accounts, so don't assert NotEmpty
	}

	[SkippableFact]
	public async Task ListApps_WithLimit()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var apps = await Client!.ListAppsAsync(limit: 5);
		Assert.NotNull(apps);
		Assert.NotNull(apps.Data);
		Assert.True(apps.Data.Count() <= 5);
	}

	[SkippableFact]
	public async Task GetAppByBundleId()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Assert.NotNull(app);
		Assert.Equal(TestBundleId, app.Attributes.BundleId);
	}

	[SkippableFact]
	public async Task GetAppByBundleId_NonExistent_ReturnsNull()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var app = await Client!.GetAppByBundleIdAsync("com.nonexistent.test.app.that.does.not.exist");
		Assert.Null(app);
	}

	// ===== APP STORE VERSION TESTS =====

	[SkippableFact]
	public async Task ListAppStoreVersions()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var versions = await Client!.ListAppStoreVersionsAsync(app!.Id, limit: 10);
		Assert.NotNull(versions);
		Assert.NotNull(versions.Data);
		// Versions may be empty for new apps
	}

	[SkippableFact]
	public async Task ListAppStoreVersions_FilterByPlatform()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var versions = await Client!.ListAppStoreVersionsAsync(
			app!.Id,
			filterPlatform: Platform.IOS,
			limit: 10);
		Assert.NotNull(versions);
		Assert.NotNull(versions.Data);

		if (versions.Data.Any())
		{
			Assert.All(versions.Data, version =>
				Assert.Equal(Platform.IOS, version.Attributes.Platform));
		}
	}

	[SkippableFact]
	public async Task GetLatestEditableAppStoreVersion()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var version = await Client!.GetLatestEditableAppStoreVersionAsync(app!.Id);
		// May be null if no editable version exists
		if (version != null)
		{
			Assert.NotNull(version.Attributes.VersionString);
			Assert.NotEmpty(version.Attributes.VersionString);
		}
	}

	// ===== APP STORE VERSION LOCALIZATION TESTS =====

	[SkippableFact]
	public async Task ListAppStoreVersionLocalizations()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var version = await Client!.GetLatestEditableAppStoreVersionAsync(app!.Id);
		Skip.If(version == null, "No editable app store version found");

		var localizations = await Client!.ListAppStoreVersionLocalizationsAsync(version!.Id);
		Assert.NotNull(localizations);
		Assert.NotNull(localizations.Data);
		// Localizations may be empty for new versions
	}

	[SkippableFact]
	public async Task GetAppStoreVersionLocalization_ByLocale()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var version = await Client!.GetLatestEditableAppStoreVersionAsync(app!.Id);
		Skip.If(version == null, "No editable app store version found");

		var localization = await Client!.GetAppStoreVersionLocalizationAsync(version!.Id, "en-US");
		// May be null if en-US localization doesn't exist
		if (localization != null)
		{
			Assert.Equal("en-US", localization.Attributes.Locale);
		}
	}

	// ===== SCREENSHOT SET TESTS =====

	[SkippableFact]
	public async Task ListAppScreenshotSets()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var version = await Client!.GetLatestEditableAppStoreVersionAsync(app!.Id);
		Skip.If(version == null, "No editable app store version found");

		var localizations = await Client!.ListAppStoreVersionLocalizationsAsync(version!.Id);
		Skip.If(!localizations.Data.Any(), "No localizations found");

		var localization = localizations.Data.First();
		var screenshotSets = await Client!.ListAppScreenshotSetsAsync(localization.Id);
		Assert.NotNull(screenshotSets);
		Assert.NotNull(screenshotSets.Data);
		// Screenshot sets may be empty
	}

	[SkippableFact]
	public async Task ListAppScreenshotSets_FilterByDisplayType()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var version = await Client!.GetLatestEditableAppStoreVersionAsync(app!.Id);
		Skip.If(version == null, "No editable app store version found");

		var localizations = await Client!.ListAppStoreVersionLocalizationsAsync(version!.Id);
		Skip.If(!localizations.Data.Any(), "No localizations found");

		var localization = localizations.Data.First();
		var screenshotSets = await Client!.ListAppScreenshotSetsAsync(
			localization.Id,
			filterScreenshotDisplayType: ScreenshotDisplayType.APP_IPHONE_69);
		Assert.NotNull(screenshotSets);
		Assert.NotNull(screenshotSets.Data);

		if (screenshotSets.Data.Any())
		{
			Assert.All(screenshotSets.Data, set =>
				Assert.Equal(ScreenshotDisplayType.APP_IPHONE_69, set.Attributes.ScreenshotDisplayType));
		}
	}

	// ===== SCREENSHOT TESTS =====

	[SkippableFact]
	public async Task ListAppScreenshots()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		Skip.If(string.IsNullOrWhiteSpace(TestBundleId), "TEST_BUNDLE_ID not configured");

		var app = await Client!.GetAppByBundleIdAsync(TestBundleId!);
		Skip.If(app == null, $"App not found for bundle ID: {TestBundleId}");

		var version = await Client!.GetLatestEditableAppStoreVersionAsync(app!.Id);
		Skip.If(version == null, "No editable app store version found");

		var localizations = await Client!.ListAppStoreVersionLocalizationsAsync(version!.Id);
		Skip.If(!localizations.Data.Any(), "No localizations found");

		var localization = localizations.Data.First();
		var screenshotSets = await Client!.ListAppScreenshotSetsAsync(localization.Id);
		Skip.If(!screenshotSets.Data.Any(), "No screenshot sets found");

		var screenshotSet = screenshotSets.Data.First();
		var screenshots = await Client!.ListAppScreenshotsAsync(screenshotSet.Id);
		Assert.NotNull(screenshots);
		Assert.NotNull(screenshots.Data);
		// Screenshots may be empty
	}

	// ===== ENUM TESTS =====

	[Fact]
	public void ScreenshotDisplayType_EnumHasExpectedValues()
	{
		// Verify common display types exist
		Assert.True(Enum.IsDefined(typeof(ScreenshotDisplayType), ScreenshotDisplayType.APP_IPHONE_69));
		Assert.True(Enum.IsDefined(typeof(ScreenshotDisplayType), ScreenshotDisplayType.APP_IPHONE_67));
		Assert.True(Enum.IsDefined(typeof(ScreenshotDisplayType), ScreenshotDisplayType.APP_IPHONE_65));
		Assert.True(Enum.IsDefined(typeof(ScreenshotDisplayType), ScreenshotDisplayType.APP_IPAD_PRO_3GEN_129));
		Assert.True(Enum.IsDefined(typeof(ScreenshotDisplayType), ScreenshotDisplayType.APP_APPLE_TV));
		Assert.True(Enum.IsDefined(typeof(ScreenshotDisplayType), ScreenshotDisplayType.APP_APPLE_VISION_PRO));
	}

	[Fact]
	public void AppScreenshotSetAttributes_ScreenshotDisplayTypeParsesCorrectly()
	{
		var attrs = new AppScreenshotSetAttributes
		{
			ScreenshotDisplayTypeValue = "APP_IPHONE_69"
		};

		Assert.Equal(ScreenshotDisplayType.APP_IPHONE_69, attrs.ScreenshotDisplayType);
	}

	[Fact]
	public void AppScreenshotSetAttributes_UnknownDisplayType_ReturnsUnknown()
	{
		var attrs = new AppScreenshotSetAttributes
		{
			ScreenshotDisplayTypeValue = "SOME_FUTURE_DISPLAY_TYPE"
		};

		Assert.Equal(ScreenshotDisplayType.Unknown, attrs.ScreenshotDisplayType);
	}

	[Fact]
	public void AppScreenshotSetAttributes_SetDisplayType_UpdatesValue()
	{
		var attrs = new AppScreenshotSetAttributes();
		attrs.ScreenshotDisplayType = ScreenshotDisplayType.APP_IPAD_PRO_3GEN_129;

		Assert.Equal("APP_IPAD_PRO_3GEN_129", attrs.ScreenshotDisplayTypeValue);
	}

	// ===== MODEL TESTS =====

	[Fact]
	public void AppAttributes_HasExpectedProperties()
	{
		var attrs = new AppAttributes
		{
			BundleId = "com.test.app",
			Name = "Test App",
			Sku = "TEST123",
			PrimaryLocale = "en-US"
		};

		Assert.Equal("com.test.app", attrs.BundleId);
		Assert.Equal("Test App", attrs.Name);
		Assert.Equal("TEST123", attrs.Sku);
		Assert.Equal("en-US", attrs.PrimaryLocale);
	}

	[Fact]
	public void AppStoreVersionAttributes_PlatformParsesCorrectly()
	{
		var attrs = new AppStoreVersionAttributes
		{
			PlatformValue = "IOS",
			VersionString = "1.0.0"
		};

		Assert.Equal(Platform.IOS, attrs.Platform);
		Assert.Equal("1.0.0", attrs.VersionString);
	}

	[Fact]
	public void AppStoreVersionLocalizationAttributes_HasExpectedProperties()
	{
		var attrs = new AppStoreVersionLocalizationAttributes
		{
			Locale = "en-US",
			Description = "Test description",
			Keywords = "test,app,keywords",
			WhatsNew = "Bug fixes and improvements"
		};

		Assert.Equal("en-US", attrs.Locale);
		Assert.Equal("Test description", attrs.Description);
		Assert.Equal("test,app,keywords", attrs.Keywords);
		Assert.Equal("Bug fixes and improvements", attrs.WhatsNew);
	}

	[Fact]
	public void CreateAppScreenshotRequestAttributes_Constructor_SetsProperties()
	{
		var attrs = new CreateAppScreenshotRequestAttributes("screenshot.png", 123456);

		Assert.Equal("screenshot.png", attrs.FileName);
		Assert.Equal(123456, attrs.FileSize);
	}

	[Fact]
	public void UpdateAppScreenshotRequestAttributes_DefaultsToUploaded()
	{
		var attrs = new UpdateAppScreenshotRequestAttributes();

		Assert.True(attrs.Uploaded);
	}
}
