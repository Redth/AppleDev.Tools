using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public enum ScreenshotDisplayType
{
	Unknown,
	APP_IPHONE_35,          // iPhone 3.5" (deprecated)
	APP_IPHONE_40,          // iPhone 4.0" (deprecated)
	APP_IPHONE_47,          // iPhone 4.7"
	APP_IPHONE_55,          // iPhone 5.5"
	APP_IPHONE_58,          // iPhone 5.8"
	APP_IPHONE_61,          // iPhone 6.1"
	APP_IPHONE_65,          // iPhone 6.5"
	APP_IPHONE_67,          // iPhone 6.7"
	APP_IPHONE_69,          // iPhone 6.9" (latest)
	APP_IPAD_97,            // iPad 9.7"
	APP_IPAD_105,           // iPad 10.5"
	APP_IPAD_PRO_129,       // iPad Pro 12.9" (1st/2nd gen)
	APP_IPAD_PRO_3GEN_129,  // iPad Pro 12.9" (3rd gen+) - This is the current one
	APP_DESKTOP,            // macOS Desktop
	APP_WATCH_SERIES_3,     // Apple Watch Series 3
	APP_WATCH_SERIES_4,     // Apple Watch Series 4+
	APP_WATCH_SERIES_7,     // Apple Watch Series 7+
	APP_WATCH_ULTRA,        // Apple Watch Ultra
	APP_APPLE_TV,           // Apple TV
	APP_APPLE_VISION_PRO    // Apple Vision Pro
}

public class AppScreenshotSetAttributes
{
	[JsonPropertyName("screenshotDisplayType")]
	public string ScreenshotDisplayTypeValue { get; set; } = string.Empty;

	[JsonIgnore]
	public ScreenshotDisplayType ScreenshotDisplayType
	{
		get => Enum.TryParse<ScreenshotDisplayType>(ScreenshotDisplayTypeValue, out var v) ? v : ScreenshotDisplayType.Unknown;
		set => ScreenshotDisplayTypeValue = value.ToString();
	}
}

public class AppScreenshotSet : Item<AppScreenshotSetAttributes>
{
	public const string TYPE = "appScreenshotSets";

	public AppScreenshotSet() : base(new AppScreenshotSetAttributes()) { }

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class AppScreenshotSetResponse : ListResponse<AppScreenshotSet, AppScreenshotSetAttributes>
{
	public AppScreenshotSetResponse() { }
}

// Create request for screenshot set
public class AppScreenshotSetRelationships
{
	[JsonPropertyName("appStoreVersionLocalization")]
	public RelationshipRequest AppStoreVersionLocalization { get; set; } = new();
}

public class CreateAppScreenshotSetRequest
{
	[JsonPropertyName("data")]
	public CreateAppScreenshotSetRequestData Data { get; set; } = new();
}

public class CreateAppScreenshotSetRequestData
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = AppScreenshotSet.TYPE;

	[JsonPropertyName("attributes")]
	public AppScreenshotSetAttributes Attributes { get; set; } = new();

	[JsonPropertyName("relationships")]
	public AppScreenshotSetRelationships Relationships { get; set; } = new();
}
