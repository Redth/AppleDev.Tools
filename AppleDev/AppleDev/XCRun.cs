namespace AppleDev;

public class XCRun
{
	public string Locate()
	{
		return "/usr/bin/xcrun";
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
