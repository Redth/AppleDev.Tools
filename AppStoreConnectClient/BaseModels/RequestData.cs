using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class RequestData<TAttributes>
{
	public RequestData(TAttributes attributes)
	{
		Attributes = attributes;
	}

	public RequestData()
	{
		Attributes = Activator.CreateInstance<TAttributes>();
	}

	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	[JsonPropertyName("attributes")]
	public TAttributes Attributes { get; set; }
}