using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class AppAttributes
{
	[JsonPropertyName("bundleId")]
	public string BundleId { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("sku")]
	public string Sku { get; set; } = string.Empty;

	[JsonPropertyName("primaryLocale")]
	public string PrimaryLocale { get; set; } = string.Empty;

	[JsonPropertyName("isOrEverWasMadeForKids")]
	public bool? IsOrEverWasMadeForKids { get; set; }

	[JsonPropertyName("subscriptionStatusUrl")]
	public string? SubscriptionStatusUrl { get; set; }

	[JsonPropertyName("subscriptionStatusUrlVersion")]
	public string? SubscriptionStatusUrlVersion { get; set; }

	[JsonPropertyName("subscriptionStatusUrlForSandbox")]
	public string? SubscriptionStatusUrlForSandbox { get; set; }

	[JsonPropertyName("availableInNewTerritories")]
	public bool? AvailableInNewTerritories { get; set; }

	[JsonPropertyName("contentRightsDeclaration")]
	public string? ContentRightsDeclaration { get; set; }
}

public class App : Item<AppAttributes>
{
	public const string TYPE = "apps";

	public App() : base(new AppAttributes()) { }

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class AppResponse : ListResponse<App, AppAttributes>
{
	public AppResponse() { }
}
