using CliWrap;
using CliWrap.Builders;

public class Keychain
{
	public const string DefaultKeychain = "login.keychain-db";

	public FileInfo Locate(string keychain)
	{
		if (Path.IsPathRooted(keychain))
			return new FileInfo(keychain);

		return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Keychains", keychain));
	}

	public Task<bool> UpdateKeychainListAsync(string keychain = DefaultKeychain, CancellationToken cancellationToken = default)
	{
		var keychainPath = Locate(keychain);

		return WrapSecurityAsync(args =>
			{
				args.Add("list-keychains");
				args.Add("-d");
				args.Add("user");
				args.Add("-s");
				args.Add(keychainPath.FullName);

				if (!Path.GetFileName(keychainPath.FullName).Equals(DefaultKeychain))
					args.Add(Locate(DefaultKeychain).FullName);
			}, cancellationToken);
	}

	public Task<bool> DeleteKeychainAsync(string keychain = DefaultKeychain, CancellationToken cancellationToken = default)
		=> WrapSecurityAsync(new[] { "delete-keychain", Locate(keychain).FullName }, cancellationToken);

	public Task<bool> ImportPkcs12Async(string file, string passphrase, string keychain = DefaultKeychain, bool allowReadToAnyApp = false, CancellationToken cancellationToken = default)
		=> WrapSecurityAsync(args =>
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
				args.Add(passphrase);
			}, cancellationToken);

	public Task<bool> SetPartitionListAsync(string password, string keychain = DefaultKeychain, CancellationToken cancellationToken = default)
		=> WrapSecurityAsync(new[] { 
				"set-key-partition-list",
				"-S",
				"apple-tool:,apple:",
				"-k",
				password,
				Locate(keychain).FullName
			}, cancellationToken);

	public Task<bool> UnlockKeychainAsync(string password, string keychain = DefaultKeychain, CancellationToken cancellationToken = default)
		=> WrapSecurityAsync(new[] { 
				"unlock-keychain", 
				"-p", 
				password, 
				Locate(keychain).FullName 
			}, cancellationToken);

	public async Task<bool> CreateKeychainAsync(string password, string keychain = DefaultKeychain, CancellationToken cancellationToken = default)
	{
		if (!await WrapSecurityAsync(new[] {
				"create-keychain",
				"-p",
				password,
				Locate(keychain).FullName
			}, cancellationToken).ConfigureAwait(false))
			return false;

		return await WrapSecurityAsync(new[] { 
				"set-keychain-settings", 
				"-lut", 
				"21600", 
				Locate(keychain).FullName 
			}, cancellationToken).ConfigureAwait(false);
	}

	Task<bool> WrapSecurityAsync(string[] args, CancellationToken cancellationToken = default)
		=> WrapSecurityAsync(b =>
		{
			foreach (var a in args)
				b.Add(a);
		}, cancellationToken);

	async Task<bool> WrapSecurityAsync(Action<ArgumentsBuilder> args, CancellationToken cancellationToken = default)
	{
		var success = false;
		try
		{
			var r = await Cli.Wrap("/usr/bin/security")
				.WithArguments(a => args(a))
				.ExecuteAsync(cancellationToken)
				.ConfigureAwait(false);

			success = r.ExitCode == 0;
		}
		catch (OperationCanceledException) { }

		return success;
	}
}