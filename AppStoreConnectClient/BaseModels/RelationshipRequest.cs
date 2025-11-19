using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Wrapper for a single relationship in API requests
/// </summary>
public class RelationshipRequest
{
	[JsonPropertyName("data")]
	public RelationshipData Data { get; set; } = new();
}
