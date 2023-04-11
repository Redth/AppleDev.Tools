using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

[System.Text.Json.Serialization.JsonConverter(typeof(JsonItemConverter))]
public class Item
{
	public Item() { }

	[JsonPropertyName("type")]
	public virtual string Type { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	public virtual string Id { get; set; } = string.Empty;

	[JsonPropertyName("relationships")]
	public virtual Dictionary<string, Relationship> Relationships { get; set; } = new();
}

public class GenericItem : Item
{
	public GenericItem() { }

}

public abstract class Item<TAttributes> : Item
{
	public Item()
	{
		Attributes = Activator.CreateInstance<TAttributes>();
	}

	public Item(TAttributes attributes)
	{
		Attributes = attributes;
	}

	[JsonPropertyName("attributes")]
	public virtual TAttributes Attributes { get; set; }
}
