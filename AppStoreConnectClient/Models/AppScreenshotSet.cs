using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public enum ScreenshotDisplayType
{
	Unknown,
	// iPhone
	APP_IPHONE_67,          // 6.7" (iPhone 14 Pro Max, 15 Pro Max, 16 Pro Max, etc.)
	APP_IPHONE_65,          // 6.5" (iPhone 14 Plus, 13 Pro Max, 12 Pro Max, 11 Pro Max, XS Max, XR)
	APP_IPHONE_61,          // 6.1" (iPhone 15, 14, 13, 12, 11, XR)
	APP_IPHONE_58,          // 5.8" (iPhone XS, X)
	APP_IPHONE_55,          // 5.5" (iPhone 8 Plus, 7 Plus, 6s Plus)
	APP_IPHONE_47,          // 4.7" (iPhone SE 2nd/3rd gen, 8, 7, 6s)
	APP_IPHONE_40,          // 4.0" (iPhone SE 1st gen, 5s, 5c, 5)
	APP_IPHONE_35,          // 3.5" (iPhone 4s, 4, 3GS, 3G, original)
	// iPad
	APP_IPAD_PRO_3GEN_129,  // iPad Pro 12.9" (3rd gen+) - Current required size
	APP_IPAD_PRO_3GEN_11,   // iPad Pro 11" (3rd gen+)
	APP_IPAD_PRO_129,       // iPad Pro 12.9" (1st/2nd gen)
	APP_IPAD_105,           // iPad 10.5"
	APP_IPAD_97,            // iPad 9.7"
	// Other Platforms
	APP_DESKTOP,            // macOS Desktop
	APP_WATCH_ULTRA,        // Apple Watch Ultra
	APP_WATCH_SERIES_10,    // Apple Watch Series 10
	APP_WATCH_SERIES_7,     // Apple Watch Series 7+
	APP_WATCH_SERIES_4,     // Apple Watch Series 4+
	APP_WATCH_SERIES_3,     // Apple Watch Series 3
	APP_APPLE_TV,           // Apple TV
	APP_APPLE_VISION_PRO,   // Apple Vision Pro
	// iMessage Apps
	IMESSAGE_APP_IPHONE_67,
	IMESSAGE_APP_IPHONE_65,
	IMESSAGE_APP_IPHONE_61,
	IMESSAGE_APP_IPHONE_58,
	IMESSAGE_APP_IPHONE_55,
	IMESSAGE_APP_IPHONE_47,
	IMESSAGE_APP_IPHONE_40,
	IMESSAGE_APP_IPAD_PRO_3GEN_129,
	IMESSAGE_APP_IPAD_PRO_3GEN_11,
	IMESSAGE_APP_IPAD_PRO_129,
	IMESSAGE_APP_IPAD_105,
	IMESSAGE_APP_IPAD_97
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
