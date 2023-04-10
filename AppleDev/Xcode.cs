using CliWrap;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppleDev;

public class Xcode
{
	public Xcode()
	{
		Logger = NullLogger<Xcode>.Instance;
	}
	
	public Xcode(ILogger<Xcode> logger)
	{
		Logger = logger;
	}
	
	public readonly ILogger<Xcode> Logger;
	
	public async Task<DirectoryInfo?> LocateAsync(CancellationToken cancellationToken = default)
	{
		if (!OperatingSystem.IsMacOS())
			return null;

		var selected = await GetSelectedXCodePathAsync(cancellationToken).ConfigureAwait(false);

		if (!string.IsNullOrEmpty(selected))
			return new DirectoryInfo(selected);

		var i = FindXCodeInstalls()?.FirstOrDefault();

		if (i is not null)
			return new DirectoryInfo(i);
		
		return null;
	}

	static async Task<string?> GetSelectedXCodePathAsync(CancellationToken cancellationToken)
	{
		var stdout = new StringBuilder();
		var stderr = new StringBuilder();

		await Cli.Wrap("/usr/bin/xcode-select")
			.WithArguments("-p")
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.ExecuteAsync(cancellationToken);

		var xcodeSelectedPath = stdout.ToString().Trim();

		if (!string.IsNullOrEmpty(xcodeSelectedPath))
		{
			var infoPlist = Path.Combine(xcodeSelectedPath, "..", "Info.plist");
			if (File.Exists(infoPlist))
			{
				var info = GetXcodeInfo(
					Path.GetFullPath(
					Path.Combine(xcodeSelectedPath, "..", "..")), true);

				if (info != null)
					return info?.Path;
			}
		}

		return null;
	}

	static readonly string[] LikelyPaths = new[]
	{
		"/Applications/Xcode.app",
		"/Applications/Xcode-beta.app",
	};

	static IEnumerable<string> FindXCodeInstalls()
	{
		foreach (var p in LikelyPaths)
		{
			var i = GetXcodeInfo(p, false)?.Path;
			if (i != null)
				yield return i;
		}
	}

	static (string Path, bool Selected)? GetXcodeInfo(string path, bool selected)
	{
		var versionPlist = Path.Combine(path, "Contents", "version.plist");

		if (File.Exists(versionPlist))
		{
			return (path, selected);
		}
		else
		{
			var infoPlist = Path.Combine(path, "Contents", "Info.plist");

			if (File.Exists(infoPlist))
			{
				return (path, selected);
			}
		}
		return null;
	}
}