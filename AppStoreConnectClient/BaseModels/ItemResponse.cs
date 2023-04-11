using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class ItemResponse<TItem, TAttributes> : Response where TItem : Item<TAttributes>
{
	public ItemResponse() : base()
	{ 
		Data = Activator.CreateInstance<TItem>();
	}

	public ItemResponse(TItem item) : base()
	{
		Data = item;
	}

	[JsonPropertyName("data")]
	public TItem Data { get; set; }

}
