using Newtonsoft.Json;

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

	[JsonProperty("simulator")]
	public bool Simulator { get; set; }

	[JsonProperty("operatingSystemVersion")]
	public string? OperatingSystemVersion { get; set; }

	[JsonProperty("available")]
	public bool Available { get; set; }

	[JsonProperty("platform")]
	public string? Platform { get; set; }

	[JsonProperty("modelCode")]
	public string? ModelCode { get; set; }

	[JsonProperty("identifier")]
	public string? Identifier { get; set; }

	[JsonProperty("architecture")]
	public string? Architecture { get; set; }

	[JsonProperty("modelUTI")]
	public string? ModelUTI { get; set; }

	[JsonProperty("modelName")]
	public string? ModelName { get; set; }

	[JsonProperty("name")]
	public string? Name { get; set; }

	[JsonProperty("interface")]
	public string? Interface { get; set; }
}
