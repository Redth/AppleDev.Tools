namespace AppleDev;

public class ALTool : XCRun
{
	public async Task UploadAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId)
	{
		CliWrap.Cli.Wrap(Locate())
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
			});
	}

	public async Task ValidateAppAsync(string appPath, ALToolAppType appType, string apiKeyId, string issuerId)
	{
		CliWrap.Cli.Wrap(Locate())
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
			});
	}
}
