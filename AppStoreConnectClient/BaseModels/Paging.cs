using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class Paging
{
	public Paging() { }

	[JsonPropertyName("total")]
	public uint Total { get; set; } = 0;

	[JsonPropertyName("limit")]
	public uint Limit { get; set; } = 2147483647;

}
