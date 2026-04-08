using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

public class AppScreenshotAttributes
{
	[JsonPropertyName("fileSize")]
	public long? FileSize { get; set; }

	[JsonPropertyName("fileName")]
	public string? FileName { get; set; }

	[JsonPropertyName("sourceFileChecksum")]
	public string? SourceFileChecksum { get; set; }

	[JsonPropertyName("imageAsset")]
	public ImageAsset? ImageAsset { get; set; }

	[JsonPropertyName("assetToken")]
	public string? AssetToken { get; set; }

	[JsonPropertyName("assetDeliveryState")]
	public string? AssetDeliveryState { get; set; }

	[JsonPropertyName("assetType")]
	public string? AssetType { get; set; }

	[JsonPropertyName("uploadOperations")]
	public List<UploadOperation>? UploadOperations { get; set; }

	[JsonPropertyName("uploaded")]
	public bool? Uploaded { get; set; }
}

public class ImageAsset
{
	[JsonPropertyName("templateUrl")]
	public string? TemplateUrl { get; set; }

	[JsonPropertyName("width")]
	public int? Width { get; set; }

	[JsonPropertyName("height")]
	public int? Height { get; set; }
}

public class UploadOperation
{
	[JsonPropertyName("method")]
	public string? Method { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("length")]
	public long? Length { get; set; }

	[JsonPropertyName("offset")]
	public long? Offset { get; set; }

	[JsonPropertyName("requestHeaders")]
	public List<RequestHeader>? RequestHeaders { get; set; }
}

public class RequestHeader
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("value")]
	public string? Value { get; set; }
}

public class AppScreenshot : Item<AppScreenshotAttributes>
{
	public const string TYPE = "appScreenshots";

	public AppScreenshot() : base(new AppScreenshotAttributes()) { }

	[JsonPropertyName("type")]
	public override string Type { get; set; } = TYPE;
}

public class AppScreenshotResponse : ListResponse<AppScreenshot, AppScreenshotAttributes>
{
	public AppScreenshotResponse() { }
}

// Create request for screenshot
public class CreateAppScreenshotRequestAttributes
{
	public CreateAppScreenshotRequestAttributes() { }

	public CreateAppScreenshotRequestAttributes(string fileName, long fileSize)
	{
		FileName = fileName;
		FileSize = fileSize;
	}

	[JsonPropertyName("fileName")]
	public string FileName { get; set; } = string.Empty;

	[JsonPropertyName("fileSize")]
	public long FileSize { get; set; }
}

public class AppScreenshotRelationships
{
	[JsonPropertyName("appScreenshotSet")]
	public RelationshipRequest AppScreenshotSet { get; set; } = new();
}

public class CreateAppScreenshotRequest
{
	[JsonPropertyName("data")]
	public CreateAppScreenshotRequestData Data { get; set; } = new();
}

public class CreateAppScreenshotRequestData
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = AppScreenshot.TYPE;

	[JsonPropertyName("attributes")]
	public CreateAppScreenshotRequestAttributes Attributes { get; set; } = new();

	[JsonPropertyName("relationships")]
	public AppScreenshotRelationships Relationships { get; set; } = new();
}

// Update request for screenshot (mark as uploaded)
public class UpdateAppScreenshotRequest
{
	[JsonPropertyName("data")]
	public UpdateAppScreenshotRequestData Data { get; set; } = new();
}

public class UpdateAppScreenshotRequestData
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = AppScreenshot.TYPE;

	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("attributes")]
	public UpdateAppScreenshotRequestAttributes Attributes { get; set; } = new();
}

public class UpdateAppScreenshotRequestAttributes
{
	[JsonPropertyName("uploaded")]
	public bool Uploaded { get; set; } = true;

	[JsonPropertyName("sourceFileChecksum")]
	public string? SourceFileChecksum { get; set; }
}
