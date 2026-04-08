using System.Security.Cryptography;

namespace AppleAppStoreConnect;

public partial class AppStoreConnectClient
{
	// ===== APP METHODS =====

	/// <summary>
	/// Find an app by bundle ID
	/// </summary>
	public async Task<App?> GetAppByBundleIdAsync(
		string bundleId,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();
		qs.Filter("bundleId", new[] { bundleId });
		qs.Limit(nameof(bundleId), null, 1, 1);

		var response = await RequestAsync<AppResponse>("apps", qs, cancellationToken);
		return response?.Data?.FirstOrDefault();
	}

	/// <summary>
	/// List apps with optional filtering and pagination
	/// </summary>
	public async Task<AppResponse> ListAppsAsync(
		string[]? filterBundleId = null,
		string[]? filterName = null,
		string[]? filterSku = null,
		string[]? filterId = null,
		int? limit = null,
		string? sort = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();
		qs.Filter("bundleId", filterBundleId);
		qs.Filter("name", filterName);
		qs.Filter("sku", filterSku);
		qs.Filter("id", filterId);
		qs.Limit(nameof(limit), null, 200, limit);
		qs.Sort(sort, "bundleId", "-bundleId", "name", "-name");

		return await RequestAsync<AppResponse>("apps", qs, cancellationToken)
			?? new AppResponse();
	}

	// ===== APP STORE VERSION METHODS =====

	/// <summary>
	/// Get app store versions for an app
	/// </summary>
	public async Task<AppStoreVersionResponse> ListAppStoreVersionsAsync(
		string appId,
		Platform? filterPlatform = null,
		string[]? filterVersionString = null,
		string[]? filterAppStoreState = null,
		string[]? filterId = null,
		int? limit = null,
		string? sort = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();
		qs.Filter("app", new[] { appId });
		if (filterPlatform.HasValue)
			qs.Filter("platform", new[] { filterPlatform.Value });
		qs.Filter("versionString", filterVersionString);
		qs.Filter("appStoreState", filterAppStoreState);
		qs.Filter("id", filterId);
		qs.Limit(nameof(limit), null, 200, limit);
		qs.Sort(sort, "createdDate", "-createdDate");

		return await RequestAsync<AppStoreVersionResponse>("appStoreVersions", qs, cancellationToken)
			?? new AppStoreVersionResponse();
	}

	/// <summary>
	/// Get the latest editable app store version for an app
	/// </summary>
	public async Task<AppStoreVersion?> GetLatestEditableAppStoreVersionAsync(
		string appId,
		Platform platform = Platform.IOS,
		CancellationToken cancellationToken = default)
	{
		// Get versions sorted by creation date (newest first)
		var versions = await ListAppStoreVersionsAsync(
			appId,
			filterPlatform: platform,
			sort: "-createdDate",
			limit: 10,
			cancellationToken: cancellationToken);

		// Find first editable version (PREPARE_FOR_SUBMISSION, DEVELOPER_REJECTED, REJECTED, METADATA_REJECTED, WAITING_FOR_REVIEW, IN_REVIEW)
		var editableStates = new[] { "PREPARE_FOR_SUBMISSION", "DEVELOPER_REJECTED", "REJECTED", "METADATA_REJECTED", "WAITING_FOR_REVIEW", "IN_REVIEW" };
		return versions.Data?.FirstOrDefault(v => editableStates.Contains(v.Attributes.AppStoreState));
	}

	// ===== APP STORE VERSION LOCALIZATION METHODS =====

	/// <summary>
	/// List localizations for an app store version
	/// </summary>
	public async Task<AppStoreVersionLocalizationResponse> ListAppStoreVersionLocalizationsAsync(
		string appStoreVersionId,
		string? filterLocale = null,
		int? limit = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();
		qs.Filter("appStoreVersion", new[] { appStoreVersionId });
		if (!string.IsNullOrEmpty(filterLocale))
			qs.Filter("locale", new[] { filterLocale });
		qs.Limit(nameof(limit), null, 200, limit);

		return await RequestAsync<AppStoreVersionLocalizationResponse>("appStoreVersionLocalizations", qs, cancellationToken)
			?? new AppStoreVersionLocalizationResponse();
	}

	/// <summary>
	/// Get a specific localization by locale
	/// </summary>
	public async Task<AppStoreVersionLocalization?> GetAppStoreVersionLocalizationAsync(
		string appStoreVersionId,
		string locale,
		CancellationToken cancellationToken = default)
	{
		var localizations = await ListAppStoreVersionLocalizationsAsync(
			appStoreVersionId,
			filterLocale: locale,
			limit: 1,
			cancellationToken: cancellationToken);

		return localizations.Data?.FirstOrDefault();
	}

	// ===== APP SCREENSHOT SET METHODS =====

	/// <summary>
	/// List screenshot sets for a localization
	/// </summary>
	public async Task<AppScreenshotSetResponse> ListAppScreenshotSetsAsync(
		string appStoreVersionLocalizationId,
		ScreenshotDisplayType? filterScreenshotDisplayType = null,
		int? limit = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();
		qs.Filter("appStoreVersionLocalization", new[] { appStoreVersionLocalizationId });
		if (filterScreenshotDisplayType.HasValue && filterScreenshotDisplayType.Value != ScreenshotDisplayType.Unknown)
			qs.Filter("screenshotDisplayType", new[] { filterScreenshotDisplayType.Value });
		qs.Limit(nameof(limit), null, 200, limit);

		return await RequestAsync<AppScreenshotSetResponse>("appScreenshotSets", qs, cancellationToken)
			?? new AppScreenshotSetResponse();
	}

	/// <summary>
	/// Create a screenshot set for a specific localization and display type
	/// </summary>
	public async Task<ItemResponse<AppScreenshotSet, AppScreenshotSetAttributes>?> CreateAppScreenshotSetAsync(
		string appStoreVersionLocalizationId,
		ScreenshotDisplayType screenshotDisplayType,
		CancellationToken cancellationToken = default)
	{
		var request = new CreateAppScreenshotSetRequest
		{
			Data = new CreateAppScreenshotSetRequestData
			{
				Attributes = new AppScreenshotSetAttributes
				{
					ScreenshotDisplayType = screenshotDisplayType
				},
				Relationships = new AppScreenshotSetRelationships
				{
					AppStoreVersionLocalization = new RelationshipRequest
					{
						Data = new RelationshipData
						{
							Type = AppStoreVersionLocalization.TYPE,
							Id = appStoreVersionLocalizationId
						}
					}
				}
			}
		};

		return await PostJsonAsync<AppScreenshotSet, AppScreenshotSetAttributes, CreateAppScreenshotSetRequest>(
			"appScreenshotSets",
			request,
			cancellationToken);
	}

	/// <summary>
	/// Delete a screenshot set
	/// </summary>
	public async Task<bool> DeleteAppScreenshotSetAsync(
		string appScreenshotSetId,
		CancellationToken cancellationToken = default)
	{
		return await DeleteAsync($"appScreenshotSets/{appScreenshotSetId}", cancellationToken);
	}

	// ===== APP SCREENSHOT METHODS =====

	/// <summary>
	/// List screenshots in a screenshot set
	/// </summary>
	public async Task<AppScreenshotResponse> ListAppScreenshotsAsync(
		string appScreenshotSetId,
		int? limit = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();
		qs.Filter("appScreenshotSet", new[] { appScreenshotSetId });
		qs.Limit(nameof(limit), null, 200, limit);

		return await RequestAsync<AppScreenshotResponse>("appScreenshots", qs, cancellationToken)
			?? new AppScreenshotResponse();
	}

	/// <summary>
	/// Reserve a screenshot upload slot
	/// </summary>
	public async Task<ItemResponse<AppScreenshot, AppScreenshotAttributes>?> ReserveAppScreenshotUploadAsync(
		string appScreenshotSetId,
		string fileName,
		long fileSize,
		CancellationToken cancellationToken = default)
	{
		var request = new CreateAppScreenshotRequest
		{
			Data = new CreateAppScreenshotRequestData
			{
				Attributes = new CreateAppScreenshotRequestAttributes(fileName, fileSize),
				Relationships = new AppScreenshotRelationships
				{
					AppScreenshotSet = new RelationshipRequest
					{
						Data = new RelationshipData
						{
							Type = AppScreenshotSet.TYPE,
							Id = appScreenshotSetId
						}
					}
				}
			}
		};

		return await PostJsonAsync<AppScreenshot, AppScreenshotAttributes, CreateAppScreenshotRequest>(
			"appScreenshots",
			request,
			cancellationToken);
	}

	/// <summary>
	/// Upload screenshot file data to the reserved upload URL
	/// </summary>
	public async Task UploadScreenshotDataAsync(
		UploadOperation uploadOperation,
		byte[] data,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(uploadOperation.Url))
			throw new ArgumentException("Upload operation URL is null or empty");

		using var content = new ByteArrayContent(data);
		content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

		// Add custom headers from upload operation
		if (uploadOperation.RequestHeaders != null)
		{
			foreach (var header in uploadOperation.RequestHeaders)
			{
				if (!string.IsNullOrEmpty(header.Name) && !string.IsNullOrEmpty(header.Value))
				{
					content.Headers.TryAddWithoutValidation(header.Name, header.Value);
				}
			}
		}

		var response = await http.PutAsync(uploadOperation.Url, content, cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Commit a screenshot upload (mark as uploaded)
	/// </summary>
	public async Task<ItemResponse<AppScreenshot, AppScreenshotAttributes>?> CommitAppScreenshotAsync(
		string appScreenshotId,
		string? sourceFileChecksum = null,
		CancellationToken cancellationToken = default)
	{
		var request = new UpdateAppScreenshotRequest
		{
			Data = new UpdateAppScreenshotRequestData
			{
				Id = appScreenshotId,
				Attributes = new UpdateAppScreenshotRequestAttributes
				{
					Uploaded = true,
					SourceFileChecksum = sourceFileChecksum
				}
			}
		};

		return await PatchJsonAsync<AppScreenshot, AppScreenshotAttributes, UpdateAppScreenshotRequest>(
			$"appScreenshots/{appScreenshotId}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// Delete a screenshot
	/// </summary>
	public async Task<bool> DeleteAppScreenshotAsync(
		string appScreenshotId,
		CancellationToken cancellationToken = default)
	{
		return await DeleteAsync($"appScreenshots/{appScreenshotId}", cancellationToken);
	}

	/// <summary>
	/// Upload a screenshot file (high-level method that handles the full workflow)
	/// </summary>
	public async Task<AppScreenshot?> UploadScreenshotAsync(
		string appScreenshotSetId,
		string filePath,
		CancellationToken cancellationToken = default)
	{
		if (!File.Exists(filePath))
			throw new FileNotFoundException("Screenshot file not found", filePath);

		var fileInfo = new FileInfo(filePath);
		var fileName = fileInfo.Name;
		var fileSize = fileInfo.Length;

		// Read file data
		var fileData = await File.ReadAllBytesAsync(filePath, cancellationToken);

		// Calculate MD5 checksum
		string checksum;
		using (var md5 = MD5.Create())
		{
			var hash = md5.ComputeHash(fileData);
			checksum = Convert.ToBase64String(hash);
		}

		// Step 1: Reserve upload slot
		var reservationResponse = await ReserveAppScreenshotUploadAsync(
			appScreenshotSetId,
			fileName,
			fileSize,
			cancellationToken);

		if (reservationResponse?.Data == null)
			throw new InvalidOperationException("Failed to reserve screenshot upload slot");

		var screenshot = reservationResponse.Data;
		var uploadOps = screenshot.Attributes.UploadOperations;

		if (uploadOps == null || uploadOps.Count == 0)
			throw new InvalidOperationException("No upload operations returned from reservation");

		// Step 2: Upload file data to each upload operation
		foreach (var uploadOp in uploadOps)
		{
			var offset = (int)(uploadOp.Offset ?? 0);
			var length = (int)(uploadOp.Length ?? fileData.Length);
			var chunk = fileData.Skip(offset).Take(length).ToArray();

			await UploadScreenshotDataAsync(uploadOp, chunk, cancellationToken);
		}

		// Step 3: Commit the upload
		var commitResponse = await CommitAppScreenshotAsync(
			screenshot.Id,
			checksum,
			cancellationToken);

		return commitResponse?.Data;
	}

	/// <summary>
	/// Get or create a screenshot set for a localization and display type
	/// </summary>
	public async Task<AppScreenshotSet?> GetOrCreateScreenshotSetAsync(
		string appStoreVersionLocalizationId,
		ScreenshotDisplayType screenshotDisplayType,
		CancellationToken cancellationToken = default)
	{
		// Try to get existing screenshot set
		var existingSets = await ListAppScreenshotSetsAsync(
			appStoreVersionLocalizationId,
			filterScreenshotDisplayType: screenshotDisplayType,
			limit: 1,
			cancellationToken: cancellationToken);

		if (existingSets.Data?.Any() == true)
			return existingSets.Data.First();

		// Create new screenshot set
		var createResponse = await CreateAppScreenshotSetAsync(
			appStoreVersionLocalizationId,
			screenshotDisplayType,
			cancellationToken);

		return createResponse?.Data;
	}
}
