using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppleDev;

public class XCRun
{
	public XCRun()
	{
		Logger = NullLogger<XCRun>.Instance;
	}
	
	public XCRun(ILogger<XCRun> logger)
	{
		Logger = logger;
	}
	
	public XCRun(ILogger logger)
	{
		Logger = logger;
	}

	protected readonly ILogger Logger;
	
	public const string ToolPath = "/usr/bin/xcrun";
	
	public FileInfo? Locate()
	{
		var path = new FileInfo(ToolPath);

		if (path.Exists)
			return path;

		return null;
	}

	internal FileInfo LocateOrThrow()
	{
		var xcrun = Locate();
		if (xcrun is null || !xcrun.Exists)
			throw new FileNotFoundException(xcrun?.FullName ?? ToolPath);
		return xcrun;
	}

	internal void ThrowIfNotMacOS()
	{
		if (!OperatingSystem.IsMacOS())
			throw new PlatformNotSupportedException();
	}

	public async Task<FileInfo> InstallPrivateKey(string apiKeyId, string privateKeyBase64)
	{
		var pkDir = PrivateKeysDirectory;
		var path = Path.Combine(pkDir.FullName, $"AuthKey_{apiKeyId}");

		await File.WriteAllTextAsync(path, privateKeyBase64);
		return new FileInfo(path);
	}

	DirectoryInfo? privateKeysDirectory;
	
	public DirectoryInfo PrivateKeysDirectory
	{
		get
		{
			privateKeysDirectory ??=
				new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
					"private_keys"));
			if (!privateKeysDirectory.Exists)
				privateKeysDirectory.Create();
			return privateKeysDirectory;
		}
		set
		{
			privateKeysDirectory = value;
		}
	}
}
