using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Handles both single object and array formats for relationship data
/// Apple API uses data: {...} for to-one relationships and data: [{...}] for to-many
/// </summary>
public class RelationshipJsonConverter : JsonConverter<Relationship>
{
	public override Relationship Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var relationship = new Relationship();
		
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject token");

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				return relationship;

			if (reader.TokenType != JsonTokenType.PropertyName)
				continue;

			var propertyName = reader.GetString();
			reader.Read();

			switch (propertyName)
			{
				case "data":
					relationship.Data = ReadRelationshipData(ref reader, options);
					break;
				case "meta":
					relationship.Meta = JsonSerializer.Deserialize<Meta>(ref reader, options) ?? new Meta();
					break;
				case "links":
					relationship.Links = JsonSerializer.Deserialize<Links>(ref reader, options) ?? new Links();
					break;
				default:
					reader.Skip();
					break;
			}
		}

		return relationship;
	}

	private List<RelationshipData>? ReadRelationshipData(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.StartArray)
		{
			// To-many relationship: data is an array
			return JsonSerializer.Deserialize<List<RelationshipData>>(ref reader, options);
		}
		else if (reader.TokenType == JsonTokenType.StartObject)
		{
			// To-one relationship: data is a single object, wrap in list
			var single = JsonSerializer.Deserialize<RelationshipData>(ref reader, options);
			return single != null ? new List<RelationshipData> { single } : null;
		}

		return null;
	}

	public override void Write(Utf8JsonWriter writer, Relationship value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value.Data != null)
		{
			writer.WritePropertyName("data");
			JsonSerializer.Serialize(writer, value.Data, options);
		}

		if (value.Meta != null)
		{
			writer.WritePropertyName("meta");
			JsonSerializer.Serialize(writer, value.Meta, options);
		}

		if (value.Links != null)
		{
			writer.WritePropertyName("links");
			JsonSerializer.Serialize(writer, value.Links, options);
		}

		writer.WriteEndObject();
	}
}

