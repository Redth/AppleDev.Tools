using System.Text.Json.Serialization;

namespace AppleDev;

public class Device
{
	public const string PlatformMacOsx = "com.apple.platform.macosx";
	public const string PlatformiPhoneSimulator = "com.apple.platform.iphonesimulator";
	public const string PlatformAppleTvSimulator = "com.apple.platform.appletvsimulator";
	public const string PlatformAppleTv = "com.apple.platform.appletvos";
	public const string PlatformWatchSimulator = "com.apple.platform.watchsimulator";
	public const string PlatformiPhone = "com.apple.platform.iphoneos";
	public const string PlatformWatch = "com.apple.platform.watchos";

	[JsonPropertyName("simulator")]
	public bool Simulator { get; set; }

	[JsonPropertyName("operatingSystemVersion")]
	public string? OperatingSystemVersion { get; set; }

	[JsonPropertyName("available")]
	public bool Available { get; set; }

	[JsonPropertyName("platform")]
	public string? Platform { get; set; }

	[JsonPropertyName("modelCode")]
	public string? ModelCode { get; set; }

	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("architecture")]
	public string? Architecture { get; set; }

	[JsonPropertyName("modelUTI")]
	public string? ModelUTI { get; set; }

	[JsonPropertyName("modelName")]
	public string? ModelName { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("interface")]
	public string? Interface { get; set; }
}
