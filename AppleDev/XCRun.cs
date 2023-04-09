namespace AppleDev;

public class XCRun
{
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

	public Task InstallPrivateKey(string apiKeyId, string privateKeyBase64)
	{
		var pkDir = PrivateKeysDirectory;
		var path = Path.Combine(pkDir.FullName, $"AuthKey_{apiKeyId}");

		return File.WriteAllTextAsync(path, privateKeyBase64);
	}

	public DirectoryInfo PrivateKeysDirectory
	{
		get
		{
			var d = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "private_keys"));
			if (!d.Exists)
				d.Create();
			return d;
		}
	}
}
