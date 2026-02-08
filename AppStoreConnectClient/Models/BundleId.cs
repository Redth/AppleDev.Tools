using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AppleAppStoreConnect;

public class BundleIdAttributes
{
	public BundleIdAttributes() { }

	[JsonPropertyName("identifier")]
	public string Identifier { get; set; } = string.Empty;

	public bool IdentifierMatches(string value)
	{
		var rx = new Regex("^" + Regex.Escape(Identifier).Replace("\\*", ".*") + "$");
		return rx.IsMatch(value);
	}

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("platform")]
	public string PlatformValue { get; set; } = string.Empty;

	[JsonIgnore]
	public Platform? Platform
	{
		get => string.IsNullOrEmpty(PlatformValue) ? null : Enum.TryParse<Platform>(PlatformValue, out var v) ? v : AppleAppStoreConnect.Platform.Unknown;
		set => PlatformValue = value?.ToString() ?? string.Empty;
	}

	[JsonPropertyName("seedId")]
	public string SeedId { get; set; } = string.Empty;
}

public class BundleId : Item<BundleIdAttributes>
{
	public const string TYPE = "bundleIds";

	public BundleId() : base(new BundleIdAttributes())
	{
	}

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;

	[JsonIgnore]
	public Relationship Profiles
		=> Relationships["profiles"];
}

public class BundleIdResponse : ListResponse<BundleId, BundleIdAttributes>
{
	public BundleIdResponse() { }

	[JsonIgnore]
	public IEnumerable<Profile> IncludedProfiles
		=> GetIncluded<Profile>();
}

public enum Platform
{
	Unknown,
	IOS,
	MAC_OS,
	UNIVERSAL,
}