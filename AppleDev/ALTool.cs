namespace AppleDev;

public class ALTool : XCRun
{
	public async Task UploadAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId, CancellationToken cancellationToken = default)
	{
		var xcrun = Locate();
		if (xcrun is null || !xcrun.Exists)
			throw new FileNotFoundException(xcrun?.FullName ?? ToolPath);
		
		await CliWrap.Cli.Wrap(xcrun.FullName)
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
			}).ExecuteAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task ValidateAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId, CancellationToken cancellationToken = default)
	{
		var xcrun = Locate();
		if (xcrun is null || !xcrun.Exists)
			throw new FileNotFoundException(xcrun?.FullName ?? ToolPath);
		
		await CliWrap.Cli.Wrap(xcrun.FullName)
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
			.ExecuteAsync(cancellationToken).ConfigureAwait(false);
	}
}
