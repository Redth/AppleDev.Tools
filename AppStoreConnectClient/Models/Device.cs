using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class DeviceAttributes
{
	public DeviceAttributes() { }
	public DeviceAttributes(string name, Platform platform, string udid)
	{
		Name = name;
		PlatformValue = platform.ToString();
		Udid = udid;
	}

	[JsonPropertyName("deviceClass")]
	public string DeviceClass { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("platform")]
	public string PlatformValue { get; set; } = string.Empty;

	[JsonIgnore]
	public Platform Platform
	{
		get => Enum.TryParse<Platform>(PlatformValue, out var v) ? v : Platform.Unknown;
		set => PlatformValue = value.ToString();
	}


	[JsonPropertyName("model")]
	public string Model { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string StatusValue { get; set; } = string.Empty;

	[JsonIgnore]
	public DeviceStatus Status
	{
		get => Enum.TryParse<DeviceStatus>(StatusValue, out var v) ? v : DeviceStatus.Unknown;
		set => StatusValue = value.ToString();
	}

	[JsonPropertyName("udid")]
	public string Udid { get; set; } = string.Empty;

	[JsonPropertyName("addedDate")]
	public DateTimeOffset? AddedDate { get; set; }
}

public class Device : Item<DeviceAttributes>
{
	public const string TYPE = "devices";

	public Device() : base(new DeviceAttributes()) { }

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class DeviceResponse : ListResponse<Device, DeviceAttributes>
{
	public DeviceResponse() { }

}

public enum DeviceStatus
{
	Unknown,
	ENABLED,
	DISABLED
}