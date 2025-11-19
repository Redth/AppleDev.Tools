using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class ApiErrorSource
{
	[JsonPropertyName("pointer")] public string? Pointer { get; set; }
}

public class ApiError
{
	[JsonPropertyName("id")] public string? Id { get; set; }
	[JsonPropertyName("status")] public string? Status { get; set; }
	[JsonPropertyName("code")] public string? Code { get; set; }
	[JsonPropertyName("title")] public string? Title { get; set; }
	[JsonPropertyName("detail")] public string? Detail { get; set; }
	[JsonPropertyName("source")] public ApiErrorSource? Source { get; set; }
}

public class AppleApiException : Exception
{
	public IReadOnlyList<ApiError> Errors { get; }
	public int HttpStatusCode { get; }
	public string RawBody { get; }

	public AppleApiException(int statusCode, string rawBody, List<ApiError> errors) : base(BuildMessage(statusCode, errors))
	{
		HttpStatusCode = statusCode;
		RawBody = rawBody;
		Errors = errors;
	}

	static string BuildMessage(int code, List<ApiError> errors)
	{
		if (errors.Count == 0) return $"HTTP {code} error";
		return $"HTTP {code}: " + string.Join("; ", errors.Select(e => $"{e.Code}: {e.Detail}"));
	}
}
