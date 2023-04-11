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

	Task<T?> RequestAsync<T>(string path, QueryStringBuilder queryString, CancellationToken cancellationToken = default)
	{
		var token = Configuration.AccessToken;

		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		return http.GetFromJsonAsync<T>(
			UrlBase.TrimEnd('/') + $"/{path}?{queryString.ToString()}",
			JsonSerializerOptions,
			cancellationToken);
	}

	async Task<ItemResponse<TItem, TAttributes>?> PostAsync<TItem, TAttributes>(string path, TAttributes attributes, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;

		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var response = await http.PostAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", attributes, cancellationToken).ConfigureAwait(false);

		return await response.Content.ReadFromJsonAsync<ItemResponse<TItem, TAttributes>>(JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
	}

	async Task<ItemResponse<TItem, TAttributes>?> PostAsync<TItem, TAttributes, TRequestAttributes>(string path, TRequestAttributes attributes, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;

		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var request = new Request<TRequestAttributes>(new RequestData<TRequestAttributes>(attributes));

		var response = await http.PostAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", request, cancellationToken).ConfigureAwait(false);

		return await response.Content.ReadFromJsonAsync<ItemResponse<TItem, TAttributes>>(JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
	}

	async Task<ItemResponse<TItem, TAttributes>?> PatchAsync<TItem, TAttributes>(string path, TAttributes attributes, CancellationToken cancellationToken = default) where TItem : Item<TAttributes> where TAttributes : class
	{
		var token = Configuration.AccessToken;

		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var response = await http.PatchAsJsonAsync(UrlBase.TrimEnd('/') + $"/{path}", attributes, cancellationToken).ConfigureAwait(false);

		return await response.Content.ReadFromJsonAsync<ItemResponse<TItem, TAttributes>>(JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
	}

	async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
	{
		var token = Configuration.AccessToken;

		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var response = await http.DeleteAsync(UrlBase.TrimEnd('/') + $"/{path}", cancellationToken).ConfigureAwait(false);

		return response.IsSuccessStatusCode;
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
