using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Wrapper for multiple relationships (array) in API requests
/// </summary>
public class RelationshipArrayRequest
{
	[JsonPropertyName("data")]
	public List<RelationshipData> Data { get; set; } = new();
}
