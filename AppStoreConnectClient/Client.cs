using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public partial class AppStoreConnectClient
{
	public const string UrlBase = "https://api.appstoreconnect.apple.com/v1/";

	public AppStoreConnectClient(AppStoreConnectConfiguration configuration)
	{
		this.Configuration = configuration;
	}

	public readonly AppStoreConnectConfiguration Configuration;


	HttpClient http = new HttpClient();

	async Task<T?> RequestAsync<T>(string path, QueryStringBuilder queryString, CancellationToken cancellationToken = default) where T : Response
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var url = UrlBase.TrimEnd('/') + $"/{path}?{queryString.ToString()}";
		var httpResponse = await http.GetAsync(url, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);
		if (!httpResponse.IsSuccessStatusCode)
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		if (parsed?.HasErrors ?? false)
			throw new AppleApiException((int)httpResponse.StatusCode, content, parsed.Errors);
		return parsed;
	}

	async Task<ItemResponse<TItem, TAttributes>?> PostAsync<TItem, TAttributes>(string path, TAttributes attributes, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var httpResponse = await http.PostAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", attributes, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<ItemResponse<TItem, TAttributes>>(content, JsonSerializerOptions);
		if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		return parsed;
	}

	async Task<ItemResponse<TItem, TAttributes>?> PostAsync<TItem, TAttributes, TRequestAttributes>(string path, TRequestAttributes attributes, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var request = new Request<TRequestAttributes>(new RequestData<TRequestAttributes>(attributes));
		// Properly extract constant TYPE field if present
		if (string.IsNullOrEmpty(request.Data.Type))
		{
			var typeField = typeof(TItem).GetField("TYPE", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
			request.Data.Type = typeField?.GetValue(null)?.ToString() ?? path; // fallback to path (e.g. devices)
		}

		var httpResponse = await http.PostAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", request, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<ItemResponse<TItem, TAttributes>>(content, JsonSerializerOptions);
		if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		return parsed;
	}

	async Task<ItemResponse<TItem, TAttributes>?> PostJsonAsync<TItem, TAttributes, TRequest>(string path, TRequest request, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var httpResponse = await http.PostAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", request, JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<ItemResponse<TItem, TAttributes>>(content, JsonSerializerOptions);
		if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		return parsed;
	}

	async Task<ItemResponse<TItem, TAttributes>?> PatchJsonAsync<TItem, TAttributes, TRequest>(string path, TRequest request, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var httpResponse = await http.PatchAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", request, JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<ItemResponse<TItem, TAttributes>>(content, JsonSerializerOptions);
		if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		return parsed;
	}

	async Task<ItemResponse<TItem, TAttributes>?> PatchAsync<TItem, TAttributes>(string path, TAttributes attributes, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var httpResponse = await http.PatchAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", attributes, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<ItemResponse<TItem, TAttributes>>(content, JsonSerializerOptions);
		if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		return parsed;
	}

	async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
	{
		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var httpResponse = await http.DeleteAsync(UrlBase.TrimEnd('/') + $"/{path}", cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		if (!httpResponse.IsSuccessStatusCode)
		{
			var errors = TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		return true;
	}

	List<ApiError> TryParseErrors(string content)
	{
		try
		{
			var doc = System.Text.Json.JsonDocument.Parse(content);
			if (doc.RootElement.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
			{
				var list = new List<ApiError>();
				foreach (var e in errorsEl.EnumerateArray())
				{
					var err = System.Text.Json.JsonSerializer.Deserialize<ApiError>(e.GetRawText(), JsonSerializerOptions);
					if (err != null) list.Add(err);
				}
				return list;
			}
		}
		catch { }
		return new List<ApiError>();
	}

	JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions
	{
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
		IgnoreReadOnlyProperties = false,
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true,
		UnknownTypeHandling= JsonUnknownTypeHandling.JsonNode,
		NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
		IncludeFields = true,
		IgnoreReadOnlyFields = false,
	};
}
