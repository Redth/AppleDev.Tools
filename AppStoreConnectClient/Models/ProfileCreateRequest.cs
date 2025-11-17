using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Attributes for profile creation (only name and profileType)
/// </summary>
public class ProfileCreateAttributes
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("profileType")]
	public string ProfileType { get; set; } = string.Empty;
}

/// <summary>
/// Request data for creating a profile with relationships
/// </summary>
public class ProfileCreateRequestData
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = Profile.TYPE;

	[JsonPropertyName("attributes")]
	public ProfileCreateAttributes Attributes { get; set; } = new();

	[JsonPropertyName("relationships")]
	public ProfileRelationships Relationships { get; set; } = new();
}

/// <summary>
/// Relationships for profile creation
/// </summary>
public class ProfileRelationships
{
	[JsonPropertyName("bundleId")]
	public RelationshipRequest BundleId { get; set; } = new();

	[JsonPropertyName("certificates")]
	public RelationshipArrayRequest Certificates { get; set; } = new();

	[JsonPropertyName("devices")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RelationshipArrayRequest? Devices { get; set; }
}

/// <summary>
/// Complete request envelope for profile creation
/// </summary>
public class ProfileCreateRequest
{
	[JsonPropertyName("data")]
	public ProfileCreateRequestData Data { get; set; } = new();
}

