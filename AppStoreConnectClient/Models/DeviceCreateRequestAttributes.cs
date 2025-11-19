using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

// Attributes allowed in a device CREATE request
public class DeviceCreateRequestAttributes
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("platform")]
	public string Platform { get; set; } = string.Empty;

	[JsonPropertyName("udid")]
	public string Udid { get; set; } = string.Empty;
}

