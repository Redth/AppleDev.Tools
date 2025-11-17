using System.Linq;
using System.Net.Http.Json;
using AppleDev;

namespace AppleAppStoreConnect;

partial class AppStoreConnectClient
{
	public const string PROFILES_TYPE = Profile.TYPE;

	public async Task InstallProfilesAsync(IEnumerable<ProfileAttributes> profiles, DirectoryInfo? path = null)
	{
		foreach (var p in profiles)
		{
			await ProvisioningProfiles.InstallProfileAsync(Convert.FromBase64String(p.ProfileContent), path).ConfigureAwait(false);
		}
	}

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
		string name,
		ProfileType profileType,
		string bundleIdId,
		string[] certificateIds,
		string[]? deviceIds = null,
		CancellationToken cancellationToken = default)
	{
		var request = new ProfileCreateRequest
		{
			Data = new ProfileCreateRequestData
			{
				Attributes = new ProfileCreateAttributes
				{
					Name = name,
					ProfileType = profileType.ToString()
				},
				Relationships = new ProfileRelationships
				{
					BundleId = new RelationshipRequest
					{
						Data = new RelationshipData { Type = BUNDLEIDS_TYPE, Id = bundleIdId }
					},
					Certificates = new RelationshipArrayRequest
					{
						Data = certificateIds.Select(id => new RelationshipData
						{
							Type = CERTIFICATES_TYPE,
							Id = id
						}).ToList()
					}
				}
			}
		};

		if (deviceIds != null && deviceIds.Length > 0)
		{
			request.Data.Relationships.Devices = new RelationshipArrayRequest
			{
				Data = deviceIds.Select(id => new RelationshipData
				{
					Type = DEVICES_TYPE,
					Id = id
				}).ToList()
			};
		}

		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		var httpResponse = await http.PostAsJsonAsync(UrlBase.TrimEnd('/') + $"/{PROFILES_TYPE}", request, cancellationToken).ConfigureAwait(false);
		var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		var parsed = System.Text.Json.JsonSerializer.Deserialize<ItemResponse<Profile, ProfileAttributes>>((string)content, JsonSerializerOptions);
		
		if (!httpResponse.IsSuccessStatusCode || (parsed?.HasErrors ?? false))
		{
			var errors = parsed?.Errors ?? TryParseErrors(content);
			throw new AppleApiException((int)httpResponse.StatusCode, content, errors);
		}
		
		return parsed ?? new ItemResponse<Profile, ProfileAttributes>();
	}

	public Task<bool> DeleteProfileAsync(string id, CancellationToken cancellationToken = default)
		=> DeleteAsync($"{PROFILES_TYPE}/{id}", cancellationToken);
}
