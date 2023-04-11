using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class Relationship
{
	public Relationship() { }

	[JsonPropertyName("meta")]
	public Meta Meta { get; set; } = new();

	[JsonPropertyName("links")]
	public Links Links { get; set; } = new();
}