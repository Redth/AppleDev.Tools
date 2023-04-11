using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class JsonItemConverter : JsonConverter<Item>
{
	public override bool CanConvert(Type objectType) =>
		objectType == typeof(Item);

	public override Item Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Check for null values
		if (reader.TokenType == JsonTokenType.Null)
			return new GenericItem();

		// Copy the current state from reader (it's a struct)
		var readerAtStart = reader;

		// Read the `className` from our JSON document
		using var jsonDocument = JsonDocument.ParseValue(ref reader);
		var jsonObject = jsonDocument.RootElement;

		var typeName = jsonObject.GetProperty("type").GetString();

		var val = typeName switch
		{
			"profiles" => JsonSerializer.Deserialize<Profile>(ref readerAtStart, options),
			"bundleIds" => JsonSerializer.Deserialize<BundleId>(ref readerAtStart, options),
			_ => JsonSerializer.Deserialize<GenericItem>(ref readerAtStart, options) as Item,
		};
		
		return val ?? new GenericItem {  Type = typeName ?? string.Empty };
	}

	public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
	{
		// No need for this one in our use case, but to just dump the object into JSON
		// (without having the className property!), we can do this:
		JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}
