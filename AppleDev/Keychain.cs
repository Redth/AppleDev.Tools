using CliWrap;

public class Keychain
{
	public const string DefaultKeychain = "login";

	public FileInfo Locate(string keychain)
	{
		if (Path.IsPathRooted(keychain))
			return new FileInfo(keychain);

		var keychainName = keychain;
		if (keychainName.EndsWith(".keychain"))
			keychainName = keychainName.Substring(0, keychain.Length - 9);

		return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Keychains", $"{keychainName}.keychain"));
	}

	public async Task UpdateKeychainList(string keychain = DefaultKeychain)
	{
		var keychainPath = Locate(keychain);

		await Cli.Wrap("security")
			.WithArguments(args =>
			{
				args.Add("list-keychains");
				args.Add("-d");
				args.Add("user");
				args.Add("-s");
				args.Add(keychainPath.FullName);

				if (!Path.GetFileName(keychainPath.FullName).Equals("login.keychain"))
					args.Add(Locate("login").FullName);
			})
			.ExecuteAsync().ConfigureAwait(false);
	}

	public async Task DeleteKeychain(string keychain = DefaultKeychain)
	{
		await CliWrap.Cli.Wrap("security")
			.WithArguments(new[] { "delete-keychain", Locate(keychain).FullName })
			.ExecuteAsync().ConfigureAwait(false);
	}

	public Task ImportPkcs12(string file, string password, string keychain = DefaultKeychain, bool allowReadToAnyApp = false)
		=> Cli.Wrap("security")
			.WithArguments(args =>
			{
				args.Add("import");
				args.Add(file);
				args.Add("-k");
				args.Add(Locate(keychain).FullName);
				args.Add("-f");
				args.Add("pkcs12");

				// Allows any app to read the keys, not a good idea if keychain is retained or was not a throwaway VM
				if (allowReadToAnyApp)
					args.Add("-A");

				args.Add("-T");
				args.Add("/usr/bin/codesign");
				args.Add("-T");
				args.Add("/usr/bin/security");
				args.Add("-P");
				args.Add(password);
			})
			.ExecuteAsync();

	public Task SetPartitionList(string password, string keychain = DefaultKeychain)
		=> Cli.Wrap("security")
			.WithArguments(new[] { 
				"set-key-partition-list",
				"-S",
				"apple-tool:,apple:",
				"-k",
				password,
				Locate(keychain).FullName
			}).ExecuteAsync();

	public Task UnlockKeychain(string password, string keychain = DefaultKeychain)
		=> Cli.Wrap("security")
			.WithArguments(new[] { 
				"unlock-keychain", 
				"-p", 
				password, 
				Locate(keychain).FullName 
			})
			.ExecuteAsync();

	public async Task CreateKeychain(string password, string keychain = DefaultKeychain)
	{
		await Cli.Wrap("security")
			.WithArguments(new[] { 
				"create-keychain", 
				"-p", 
				password, 
				Locate(keychain).FullName 
			})
			.ExecuteAsync().ConfigureAwait(false);

		await Cli.Wrap("security")
			.WithArguments(new[] { 
				"set-keychain-settings", 
				"-lut", 
				"21600", 
				Locate(keychain).FullName 
			})
			.ExecuteAsync().ConfigureAwait(false);
	}
}