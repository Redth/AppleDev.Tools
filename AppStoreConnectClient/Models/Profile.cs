using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;


public class ProfileAttributes
{
	public ProfileAttributes() { }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("uuid")]
	public string Uuid { get; set; } = string.Empty;

	[JsonPropertyName("profileType")]
	public string ProfileTypeValue { get; set; } = string.Empty;

	[JsonIgnore]
	public ProfileType ProfileType
	{
		get => Enum.TryParse<ProfileType>(ProfileTypeValue, out var v) ? v : ProfileType.Unknown;
		set => value.ToString();
	}


	[JsonPropertyName("profileState")]
	public string ProfileStateValue { get; set; } = string.Empty;

	[JsonIgnore]
	public ProfileState ProfileState
	{
		get => Enum.TryParse<ProfileState>(ProfileStateValue, out var v) ? v : ProfileState.Unknown;
		set => value.ToString();
	}

	[JsonPropertyName("platform")]
	public string PlatformValue { get; set; } = string.Empty;

	[JsonIgnore]
	public Platform Platform
	{
		get => Enum.TryParse<Platform>(PlatformValue, out var v) ? v : Platform.Unknown;
		set => value.ToString();
	}


	[JsonPropertyName("createdDate")]
	public DateTimeOffset? CreatedDate { get; set; }

	[JsonPropertyName("expirationDate")]
	public DateTimeOffset? ExpirationDate { get; set; }

	[JsonPropertyName("profileContent")]
	public string ProfileContent { get; set; } = string.Empty;
}

public class Profile : Item<ProfileAttributes>
{
	public const string TYPE = "profiles";

	public Profile()
		: base(new ProfileAttributes())
	{
	}

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class ProfileResponse : ListResponse<Profile, ProfileAttributes>
{
	public ProfileResponse() { }

	[JsonIgnore]
	public IEnumerable<BundleId> IncludedBundleIds
		=> GetIncluded<BundleId>();

	[JsonIgnore]
	public IEnumerable<Certificate> IncludedCertificates
		=> GetIncluded<Certificate>();

	[JsonIgnore]
	public IEnumerable<Device> IncludedDevices
		=> GetIncluded<Device>();
}
	
public enum ProfileType
{
	Unknown,
	IOS_APP_DEVELOPMENT,
	IOS_APP_STORE,
	IOS_APP_ADHOC,
	IOS_APP_INHOUSE,
	MAC_APP_DEVELOPMENT,
	MAC_APP_STORE,
	MAC_APP_DIRECT,
	TVOS_APP_DEVELOPMENT,
	TVOS_APP_STORE,
	TVOS_APP_ADHOC,
	TVOS_APP_INHOUSE,
	MAC_CATALYST_APP_DEVELOPMENT,
	MAC_CATALYST_APP_STORE,
	MAC_CATALYST_APP_DIRECT
}

public enum ProfileState
{
	Unknown,
	ACTIVE,
	INVALID
}