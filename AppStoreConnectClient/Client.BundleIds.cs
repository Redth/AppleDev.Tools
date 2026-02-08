using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AppleAppStoreConnect;

partial class AppStoreConnectClient
{
	public const string BUNDLEIDS_TYPE = BundleId.TYPE;

	public async Task<BundleIdResponse> ListBundleIdsAsync(
		string[]? filterId = null,
		string[]? filterIdentifier = null,
		string[]? filterName = null,
		Platform[]? filterPlatform = null,
		string[]? filterSeedId = null,
		string? include = null,
		string? sort = null,
		int? limit = null,
		int? limitProfiles = null,
		int? limitBundleIdCapabilities = null,
		string[]? fieldsBundleIds = null,
		string[]? fieldsProfiles = null,
		string[]? fieldBundleIdCapabilities = null,
		string[]? fieldsApps = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();

		qs.Filter("id", filterId);
		qs.Filter("identifier", filterIdentifier);
		qs.Filter("name", filterName);
		qs.Filter("platform", filterPlatform);
		qs.Filter("seedId", filterSeedId);

		qs.Include(include, "app", "bundleIdCapabilities", PROFILES_TYPE);
		qs.Sort(sort, "id", "-id", "identifier", "-identifier", "name", "-name", "platform", "-platform", "seedId", "-seedId");

		qs.Limit(nameof(limit), null, 200, limit);
		qs.Limit(nameof(limitProfiles), PROFILES_TYPE, 50, limitProfiles);
		qs.Limit(nameof(limitBundleIdCapabilities), "bundleIdCapabilities", 50, limitBundleIdCapabilities);

		qs.Fields(BUNDLEIDS_TYPE, fieldsBundleIds);
		qs.Fields(PROFILES_TYPE, fieldsProfiles);
		qs.Fields("bundleIdCapabilities", fieldBundleIdCapabilities);
		qs.Fields("apps", fieldsApps);

		var response = await RequestAsync<BundleIdResponse>(BUNDLEIDS_TYPE, qs, cancellationToken).ConfigureAwait(false)
			?? new BundleIdResponse();

		// Apply client-side platform filter if the server didn't properly filter
		// This is necessary because the Apple API doesn't always honor the platform filter parameter
		if (filterPlatform is not null && filterPlatform.Length > 0 && response.Data is not null)
		{
			var platformNames = filterPlatform.Select(p => p.ToString()).ToHashSet();
			response.Data = response.Data.Where(b => platformNames.Contains(b.Attributes.PlatformValue)).ToList();
		}

		return response;
	}

	public async Task<ItemResponse<BundleId, BundleIdAttributes>> CreateBundleIdAsync(
		BundleIdAttributes bundleIdAttributes,
		CancellationToken cancellationToken = default)
	{
		return await PostAsync<BundleId, BundleIdAttributes, BundleIdAttributes>(BUNDLEIDS_TYPE, bundleIdAttributes, cancellationToken).ConfigureAwait(false)
			?? new ItemResponse<BundleId, BundleIdAttributes>();
	}

	public async Task<ItemResponse<BundleId, BundleIdAttributes>> UpdateBundleIdAsync(
		string id,
		BundleIdAttributes bundleIdAttributes,
		CancellationToken cancellationToken = default)
	{
		var request = new Request<BundleIdAttributes>(new RequestData<BundleIdAttributes>(bundleIdAttributes) { Type = BUNDLEIDS_TYPE });
		return await PatchJsonAsync<BundleId, BundleIdAttributes, Request<BundleIdAttributes>>($"{BUNDLEIDS_TYPE}/{id}", request, cancellationToken).ConfigureAwait(false)
			?? new ItemResponse<BundleId, BundleIdAttributes>();
	}

	public Task<bool> DeleteBundleIdAsync(string id, CancellationToken cancellationToken = default)
		=> DeleteAsync($"{BUNDLEIDS_TYPE}/{id}", cancellationToken);
}
