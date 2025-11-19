using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Represents a single relationship reference in API requests
/// </summary>
public class RelationshipData
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;
}
