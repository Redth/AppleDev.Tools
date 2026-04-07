using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class AppStoreVersionAttributes
{
	[JsonPropertyName("platform")]
	public string PlatformValue { get; set; } = string.Empty;

	[JsonIgnore]
	public Platform? Platform
	{
		get => string.IsNullOrEmpty(PlatformValue) ? null : Enum.TryParse<Platform>(PlatformValue, out var v) ? v : AppleAppStoreConnect.Platform.Unknown;
		set => PlatformValue = value?.ToString() ?? string.Empty;
	}

	[JsonPropertyName("versionString")]
	public string VersionString { get; set; } = string.Empty;

	[JsonPropertyName("appStoreState")]
	public string AppStoreState { get; set; } = string.Empty;

	[JsonPropertyName("copyright")]
	public string? Copyright { get; set; }

	[JsonPropertyName("releaseType")]
	public string? ReleaseType { get; set; }

	[JsonPropertyName("earliestReleaseDate")]
	public DateTimeOffset? EarliestReleaseDate { get; set; }

	[JsonPropertyName("downloadable")]
	public bool? Downloadable { get; set; }

	[JsonPropertyName("createdDate")]
	public DateTimeOffset? CreatedDate { get; set; }
}

public class AppStoreVersion : Item<AppStoreVersionAttributes>
{
	public const string TYPE = "appStoreVersions";

	public AppStoreVersion() : base(new AppStoreVersionAttributes()) { }

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class AppStoreVersionResponse : ListResponse<AppStoreVersion, AppStoreVersionAttributes>
{
	public AppStoreVersionResponse() { }
}
