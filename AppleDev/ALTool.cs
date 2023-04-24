using CliWrap;
using Microsoft.Extensions.Logging;
using System.Text;

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
	public async Task<ProcessResult> UploadAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId, CancellationToken cancellationToken = default)
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
				"xml",
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

		return new ProcessResult(r.ExitCode == 0, stdout.ToString(), stderr.ToString());
	}

	public async Task<ProcessResult> ValidateAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId, CancellationToken cancellationToken = default)
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
				"xml",
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

		return new ProcessResult(r.ExitCode == 0, stdout.ToString(), stderr.ToString());
	}
}
