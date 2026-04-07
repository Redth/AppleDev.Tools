using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class AppStoreVersionLocalizationAttributes
{
	[JsonPropertyName("locale")]
	public string Locale { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("keywords")]
	public string? Keywords { get; set; }

	[JsonPropertyName("marketingUrl")]
	public string? MarketingUrl { get; set; }

	[JsonPropertyName("promotionalText")]
	public string? PromotionalText { get; set; }

	[JsonPropertyName("supportUrl")]
	public string? SupportUrl { get; set; }

	[JsonPropertyName("whatsNew")]
	public string? WhatsNew { get; set; }
}

public class AppStoreVersionLocalization : Item<AppStoreVersionLocalizationAttributes>
{
	public const string TYPE = "appStoreVersionLocalizations";

	public AppStoreVersionLocalization() : base(new AppStoreVersionLocalizationAttributes()) { }

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class AppStoreVersionLocalizationResponse : ListResponse<AppStoreVersionLocalization, AppStoreVersionLocalizationAttributes>
{
	public AppStoreVersionLocalizationResponse() { }
}
