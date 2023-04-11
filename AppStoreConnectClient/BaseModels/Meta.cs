using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class Meta
{
	public Meta() { }

	[JsonPropertyName("paging")]
	public Paging Paging { get; set; } = new();
}
