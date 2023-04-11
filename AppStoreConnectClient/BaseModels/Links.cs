using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class Links
{
	public Links() { }

	[JsonPropertyName("self")]
	public Uri? Self { get; set; }

	[JsonPropertyName("related")]
	public Uri? Related { get; set; }
}
