using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Request attributes for creating a provisioning profile
/// </summary>
public class ProfileCreateRequestAttributes
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("profileType")]
	public string ProfileType { get; set; } = string.Empty;

	[JsonPropertyName("bundleId")]
	public RelationshipRequest BundleId { get; set; } = new();

	[JsonPropertyName("certificates")]
	public RelationshipArrayRequest Certificates { get; set; } = new();

	[JsonPropertyName("devices")]
	public RelationshipArrayRequest? Devices { get; set; }
}
