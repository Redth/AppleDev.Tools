using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class ListResponse<TItem, TAttributes> : Response where TItem : Item<TAttributes>
{
	public ListResponse() : base() { }

	[JsonPropertyName("data")]
	public List<TItem> Data { get; set; } = new();
}
