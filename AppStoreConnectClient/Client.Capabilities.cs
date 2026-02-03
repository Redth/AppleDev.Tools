using System.Net.Http.Json;
using System.Text.Json;

namespace AppleAppStoreConnect;

partial class AppStoreConnectClient
{
    public const string BUNDLEIDCAPABILITIES_TYPE = BundleIdCapability.TYPE;

    /// <summary>
    /// List all capabilities for a specific Bundle ID
    /// </summary>
    /// <param name="bundleIdResourceId">The resource ID of the Bundle ID (not the identifier string)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of capabilities for the Bundle ID</returns>
    /// <remarks>
    /// Note: The relationship endpoint does not support filter or limit parameters.
    /// All capabilities for the bundle ID are returned.
    /// </remarks>
    public async Task<BundleIdCapabilityResponse> ListBundleIdCapabilitiesAsync(
        string bundleIdResourceId,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();

        // Note: filter and limit parameters are not supported for this relationship endpoint

        var response = await RequestAsync<BundleIdCapabilityResponse>(
            $"{BUNDLEIDS_TYPE}/{bundleIdResourceId}/{BUNDLEIDCAPABILITIES_TYPE}", 
            qs, 
            cancellationToken).ConfigureAwait(false);

        return response ?? new BundleIdCapabilityResponse();
    }

    /// <summary>
    /// Enable a capability for a Bundle ID
    /// </summary>
    /// <param name="bundleIdResourceId">The resource ID of the Bundle ID</param>
    /// <param name="capabilityType">The type of capability to enable</param>
    /// <param name="settings">Optional settings for the capability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created capability</returns>
    public async Task<ItemResponse<BundleIdCapability, BundleIdCapabilityAttributes>> EnableCapabilityAsync(
        string bundleIdResourceId,
        CapabilityType capabilityType,
        List<CapabilitySettingRequest>? settings = null,
        CancellationToken cancellationToken = default)
    {
        var token = Configuration.AccessToken;
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new BundleIdCapabilityCreateRequest
        {
            Data = new BundleIdCapabilityCreateRequestData
            {
                Type = BUNDLEIDCAPABILITIES_TYPE,
                Attributes = new BundleIdCapabilityCreateRequestAttributes
                {
                    CapabilityType = capabilityType.ToString(),
                    Settings = settings
                },
                Relationships = new BundleIdCapabilityRelationships
                {
                    BundleId = new BundleIdRelationship
                    {
                        Data = new BundleIdRelationshipData
                        {
                            Type = BUNDLEIDS_TYPE,
                            Id = bundleIdResourceId
                        }
                    }
                }
            }
        };

        var httpResponse = await http.PostAsJsonAsync(
            UrlBase.TrimEnd('/') + $"/{BUNDLEIDCAPABILITIES_TYPE}", 
            request, 
            JsonSerializerOptions,
            cancellationToken).ConfigureAwait(false);

        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var parsed = JsonSerializer.Deserialize<ItemResponse<BundleIdCapability, BundleIdCapabilityAttributes>>(content, JsonSerializerOptions);

        if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
        {
            var errors = parsed?.Errors ?? TryParseErrors(content);
            throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
        }

        return parsed ?? new ItemResponse<BundleIdCapability, BundleIdCapabilityAttributes>();
    }

    /// <summary>
    /// Update a capability's configuration
    /// </summary>
    /// <param name="capabilityId">The ID of the capability to update</param>
    /// <param name="capabilityType">The capability type</param>
    /// <param name="settings">Updated settings for the capability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated capability</returns>
    public async Task<ItemResponse<BundleIdCapability, BundleIdCapabilityAttributes>> UpdateCapabilityAsync(
        string capabilityId,
        CapabilityType capabilityType,
        List<CapabilitySettingRequest>? settings = null,
        CancellationToken cancellationToken = default)
    {
        var token = Configuration.AccessToken;
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new BundleIdCapabilityUpdateRequest
        {
            Data = new BundleIdCapabilityUpdateRequestData
            {
                Type = BUNDLEIDCAPABILITIES_TYPE,
                Id = capabilityId,
                Attributes = new BundleIdCapabilityCreateRequestAttributes
                {
                    CapabilityType = capabilityType.ToString(),
                    Settings = settings
                }
            }
        };

        var httpResponse = await http.PatchAsJsonAsync(
            UrlBase.TrimEnd('/') + $"/{BUNDLEIDCAPABILITIES_TYPE}/{capabilityId}", 
            request, 
            JsonSerializerOptions,
            cancellationToken).ConfigureAwait(false);

        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var parsed = JsonSerializer.Deserialize<ItemResponse<BundleIdCapability, BundleIdCapabilityAttributes>>(content, JsonSerializerOptions);

        if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
        {
            var errors = parsed?.Errors ?? TryParseErrors(content);
            throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
        }

        return parsed ?? new ItemResponse<BundleIdCapability, BundleIdCapabilityAttributes>();
    }

    /// <summary>
    /// Disable/delete a capability from a Bundle ID
    /// </summary>
    /// <param name="capabilityId">The ID of the capability to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    public Task<bool> DisableCapabilityAsync(string capabilityId, CancellationToken cancellationToken = default)
        => DeleteAsync($"{BUNDLEIDCAPABILITIES_TYPE}/{capabilityId}", cancellationToken);

    /// <summary>
    /// Get available capability types that can be enabled
    /// </summary>
    /// <returns>List of all capability types</returns>
    public static IReadOnlyList<CapabilityType> GetAvailableCapabilityTypes()
    {
        return Enum.GetValues<CapabilityType>()
            .Where(c => c != CapabilityType.Unknown)
            .ToList();
    }
}
