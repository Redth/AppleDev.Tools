using System;
using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class Request<TAttributes>
{
	public Request()
	{
		Data = Activator.CreateInstance<RequestData<TAttributes>>();
	}

	public Request(RequestData<TAttributes> data)
	{
		Data = data;
	}

	[JsonPropertyName("data")]

	public RequestData<TAttributes> Data { get; set; }
}
