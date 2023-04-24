using CliWrap;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Serialization;

namespace AppleDev;

public class ALTool : XCRun
{
	public ALTool() : base() { }
	
	public ALTool(ILogger<ALTool> logger) : base(logger) { }
	
	/// <summary>
	/// Uploads an app to AppStoreConnect / TestFlight
	/// </summary>
	/// <param name="appPath">Path to the .app / .ipa to upload</param>
	/// <param name="appType">What type of app is being uploaded</param>
	/// <param name="apiKeyId">API KeyID</param>
	/// <param name="issuerId">IssuerID for the API Key</param>
	/// <param name="cancellationToken"></param>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="InvalidDataException"></exception>
	public async Task<ACToolResponse> UploadAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();
		
		var xcrun = Locate();
		if (xcrun is null || !xcrun.Exists)
			throw new FileNotFoundException(xcrun?.FullName ?? ToolPath);

		var stdout = new StringBuilder();
		var stderr = new StringBuilder();

		var r = await Cli.Wrap(xcrun.FullName)
			.WithArguments(new string[]
			{
				"altool",
				"--output-format",
				"json",
				"--upload-app",
				"--file",
				appPath,
				"--type",
				appType switch
				{ 
					ALToolAppType.iOS => "ios",
					ALToolAppType.macOS => "osx",
					ALToolAppType.watchOS => "watchos",
					ALToolAppType.tvOS => "appletvos",
					_ => throw new InvalidDataException(),
				},
				"--apiKey",
				apiKeyId,
				"--apiIssuer",
				issuerId
			})
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		return System.Text.Json.JsonSerializer.Deserialize<ACToolResponse>(stdout.ToString())!;
	}

	public async Task<ACToolResponse> ValidateAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId, CancellationToken cancellationToken = default)
	{
		base.ThrowIfNotMacOS();

		var xcrun = Locate();
		if (xcrun is null || !xcrun.Exists)
			throw new FileNotFoundException(xcrun?.FullName ?? ToolPath);

		var stdout = new StringBuilder();
		var stderr = new StringBuilder();

		var r = await Cli.Wrap(xcrun.FullName)
			.WithArguments(new string[]
			{
				"altool",
				"--output-format",
				"json",
				"--validate-app",
				"--file",
				appPath,
				"--type",
				appType switch
				{
					ALToolAppType.iOS => "ios",
					ALToolAppType.macOS => "osx",
					ALToolAppType.watchOS => "watchos",
					ALToolAppType.tvOS => "appletvos",
					_ => throw new InvalidDataException(),
				},
				"--apiKey",
				apiKeyId,
				"--apiIssuer",
				issuerId
			})
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		return System.Text.Json.JsonSerializer.Deserialize<ACToolResponse>(stdout.ToString())!;
	}
}

public class ACToolResponse
{
	[JsonIgnore]
	public bool Success
		=> Errors is null || Errors.Count <= 0;

	[JsonPropertyName("tool-version")]
	public string? ToolVersion { get; set; }

	[JsonPropertyName("tool-path")]
	public string? ToolPath { get; set; }

	[JsonPropertyName("os-version")]
	public string? OsVersion { get; set; }

	[JsonPropertyName("product-errors")]
	public List<ACToolProductError> Errors { get; set; } = new();

}

public class ACToolProductError
{
	[JsonPropertyName("message")]
	public string? Message { get; set; }

	[JsonPropertyName("code")]
	public long Code { get; set; } = 0;

	[JsonPropertyName("userInfo")]
	public ACToolProductErrorUserInfo? UserInfo { get; set; }

}

public class  ACToolProductErrorUserInfo
{
	[JsonPropertyName("NSLocalizedFailureReason")]
	public string? FailureReason { get; set; }

	[JsonPropertyName("NSLocalizedDescription")]
	public string? Description { get; set; }
}
