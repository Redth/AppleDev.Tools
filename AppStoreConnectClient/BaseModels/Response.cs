using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public abstract class Response
{
	public Response() { }

	[JsonPropertyName("included")]
	public List<Item> Included { get; set; } = new();

	[JsonPropertyName("errors")]
	public List<ApiError> Errors { get; set; } = new();

	public bool HasErrors => Errors?.Count > 0;

	public IEnumerable<TItem> GetIncluded<TItem>() where TItem : Item
	{
		return Included?.Where(i => i is TItem)?.Cast<TItem>() ?? Enumerable.Empty<TItem>();
	}
}
