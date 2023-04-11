using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleAppStoreConnect;

partial class AppStoreConnectClient
{
	public const string PROFILES_TYPE = Profile.TYPE;

	public async Task<ProfileResponse> ListProfilesAsync(
		string[]? filterId = null,
		string[]? filterName = null,
		ProfileState[]? filterProfileState = null,
		ProfileType[]? filterProfileType = null,
		string? include = null,
		string? sort = null,
		int? limit = null,
		int? limitCertificates = null,
		int? limitDevices = null,
		string[]? fieldsProfiles = null,
		string[]? fieldsBundleIds = null,
		string[]? fieldsCertificates = null,
		string[]? fieldsDevices = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();

		qs.Filter("id", filterId);
		qs.Filter("name", filterName);
		qs.Filter("profileState", filterProfileState);
		qs.Filter("profileType", filterProfileType);

		qs.Include(include, "bundleId", CERTIFICATES_TYPE, DEVICES_TYPE);
		qs.Sort(sort, "id", "-id", "name", "-name", "profileState", "-profileState", "profileType", "-profileType");

		qs.Limit(nameof(limit), null, 200, limit);
		qs.Limit(nameof(limitCertificates), CERTIFICATES_TYPE, 50, limitCertificates);
		qs.Limit(nameof(limitDevices), DEVICES_TYPE, 50, limitDevices);

		qs.Fields(BUNDLEIDS_TYPE, fieldsBundleIds);
		qs.Fields(PROFILES_TYPE, fieldsProfiles);
		qs.Fields(CERTIFICATES_TYPE, fieldsCertificates);
		qs.Fields(DEVICES_TYPE, fieldsDevices);

		return await RequestAsync<ProfileResponse>(PROFILES_TYPE, qs, cancellationToken).ConfigureAwait(false)
			?? new ProfileResponse();
	}

	public async Task<ItemResponse<Profile, ProfileAttributes>> CreateProfileAsync(
		ProfileAttributes profileAttributes,
		CancellationToken cancellationToken = default)
	{
		return await PostAsync<Profile, ProfileAttributes>(PROFILES_TYPE, profileAttributes, cancellationToken).ConfigureAwait(false)
			?? new ItemResponse<Profile, ProfileAttributes>();
	}

	public Task<bool> DeleteProfileAsync(string id, CancellationToken cancellationToken = default)
		=> DeleteAsync($"{PROFILES_TYPE}/{id}", cancellationToken);
}
