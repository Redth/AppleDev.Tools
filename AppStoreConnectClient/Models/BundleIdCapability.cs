using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

/// <summary>
/// Represents a capability type that can be enabled for a Bundle ID
/// </summary>
public enum CapabilityType
{
    Unknown,
    ICLOUD,
    IN_APP_PURCHASE,
    GAME_CENTER,
    PUSH_NOTIFICATIONS,
    WALLET,
    INTER_APP_AUDIO,
    MAPS,
    ASSOCIATED_DOMAINS,
    PERSONAL_VPN,
    APP_GROUPS,
    HEALTHKIT,
    HOMEKIT,
    WIRELESS_ACCESSORY_CONFIGURATION,
    APPLE_PAY,
    DATA_PROTECTION,
    SIRIKIT,
    NETWORK_EXTENSIONS,
    MULTIPATH,
    HOT_SPOT,
    NFC_TAG_READING,
    CLASSKIT,
    AUTOFILL_CREDENTIAL_PROVIDER,
    ACCESS_WIFI_INFORMATION,
    NETWORK_CUSTOM_PROTOCOL,
    COREMEDIA_HLS_LOW_LATENCY,
    SYSTEM_EXTENSION_INSTALL,
    USER_MANAGEMENT,
    APPLE_ID_AUTH,
    FONT_INSTALLATION,
    CARPLAY_PLAYABLE_CONTENT,
    MARZIPAN,
    SIGN_IN_WITH_APPLE,
    APP_ATTEST,
    WEATHERKIT,
    COMMUNICATION_NOTIFICATIONS,
    GROUP_ACTIVITIES,
    HEALTH_KIT_RECALIBRATE_ESTIMATES,
    TIME_SENSITIVE_NOTIFICATIONS,
    FAMILY_CONTROLS,
    DRIVER_KIT,
    EXPOSURE_NOTIFICATION,
    EXTENDED_VIRTUAL_ADDRESSING,
    FILE_PROVIDER_TESTING_MODE,
    MDMMANAGED_ASSOCIATED_DOMAINS,
    SHARED_WITH_YOU,
    INCREASED_MEMORY_LIMIT,
    KEYCHAIN_SHARING,
    MEDIA_DEVICE_DISCOVERY,
    USERNOTIFICATIONS_COMMUNICATION,
    USERNOTIFICATIONS_TIME_SENSITIVE
}

/// <summary>
/// Attributes for a Bundle ID capability
/// </summary>
public class BundleIdCapabilityAttributes
{
    [JsonPropertyName("capabilityType")]
    public string CapabilityTypeValue { get; set; } = string.Empty;

    [JsonIgnore]
    public CapabilityType CapabilityType
    {
        get => Enum.TryParse<CapabilityType>(CapabilityTypeValue, out var v) ? v : CapabilityType.Unknown;
        set => CapabilityTypeValue = value.ToString();
    }

    [JsonPropertyName("settings")]
    public List<CapabilitySetting>? Settings { get; set; }
}

/// <summary>
/// Setting for a capability
/// </summary>
public class CapabilitySetting
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("enabledByDefault")]
    public bool? EnabledByDefault { get; set; }

    [JsonPropertyName("visible")]
    public bool? Visible { get; set; }

    [JsonPropertyName("allowedInstances")]
    public string? AllowedInstances { get; set; }

    [JsonPropertyName("minInstances")]
    public int? MinInstances { get; set; }

    [JsonPropertyName("options")]
    public List<CapabilityOption>? Options { get; set; }
}

/// <summary>
/// Option for a capability setting
/// </summary>
public class CapabilityOption
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("enabledByDefault")]
    public bool? EnabledByDefault { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("supportsWildcard")]
    public bool? SupportsWildcard { get; set; }
}

/// <summary>
/// A Bundle ID capability resource
/// </summary>
public class BundleIdCapability : Item<BundleIdCapabilityAttributes>
{
    public const string TYPE = "bundleIdCapabilities";

    public BundleIdCapability() : base(new BundleIdCapabilityAttributes())
    {
    }

    [JsonPropertyName("type")]
    public override string Type { get; set; } = TYPE;
}

/// <summary>
/// Response containing a list of Bundle ID capabilities
/// </summary>
public class BundleIdCapabilityResponse : ListResponse<BundleIdCapability, BundleIdCapabilityAttributes>
{
    public BundleIdCapabilityResponse() { }
}

/// <summary>
/// Attributes for creating a Bundle ID capability
/// </summary>
public class BundleIdCapabilityCreateRequestAttributes
{
    [JsonPropertyName("capabilityType")]
    public string CapabilityType { get; set; } = string.Empty;

    [JsonPropertyName("settings")]
    public List<CapabilitySettingRequest>? Settings { get; set; }
}

/// <summary>
/// Setting request for creating/updating a capability
/// </summary>
public class CapabilitySettingRequest
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<CapabilityOptionRequest>? Options { get; set; }
}

/// <summary>
/// Option request for a capability setting
/// </summary>
public class CapabilityOptionRequest
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}

/// <summary>
/// Request body for creating a Bundle ID capability
/// </summary>
public class BundleIdCapabilityCreateRequest
{
    [JsonPropertyName("data")]
    public BundleIdCapabilityCreateRequestData Data { get; set; } = new();
}

public class BundleIdCapabilityCreateRequestData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = BundleIdCapability.TYPE;

    [JsonPropertyName("attributes")]
    public BundleIdCapabilityCreateRequestAttributes Attributes { get; set; } = new();

    [JsonPropertyName("relationships")]
    public BundleIdCapabilityRelationships Relationships { get; set; } = new();
}

public class BundleIdCapabilityRelationships
{
    [JsonPropertyName("bundleId")]
    public BundleIdRelationship BundleId { get; set; } = new();
}

public class BundleIdRelationship
{
    [JsonPropertyName("data")]
    public BundleIdRelationshipData Data { get; set; } = new();
}

public class BundleIdRelationshipData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = BundleId.TYPE;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Request body for updating a Bundle ID capability
/// </summary>
public class BundleIdCapabilityUpdateRequest
{
    [JsonPropertyName("data")]
    public BundleIdCapabilityUpdateRequestData Data { get; set; } = new();
}

public class BundleIdCapabilityUpdateRequestData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = BundleIdCapability.TYPE;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("attributes")]
    public BundleIdCapabilityCreateRequestAttributes Attributes { get; set; } = new();
}
