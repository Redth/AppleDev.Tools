using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public abstract class Response
{
	public Response() { }

	[JsonPropertyName("included")]
	public List<Item> Included { get; set; } = new();

	public IEnumerable<TItem> GetIncluded<TItem>() where TItem : Item
	{
		return Included?.Where(i => i is TItem)?.Cast<TItem>() ?? Enumerable.Empty<TItem>();
	}
}
