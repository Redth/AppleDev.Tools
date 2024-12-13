using CliWrap;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Claunia.PropertyList;

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

	public async Task<XcodeInfo?> LocateBestAsync(CancellationToken cancellationToken = default)
	{
		var all = await LocateAllAsync(cancellationToken).ConfigureAwait(false);

		var selected = all.FirstOrDefault(x => x.Selected);
		if (selected is not null)
			return selected;

		return all.OrderByDescending(x => x.Version).FirstOrDefault();
	}

	public async Task<IReadOnlyList<XcodeInfo>> LocateAllAsync(CancellationToken cancellationToken = default)
	{
		if (!OperatingSystem.IsMacOS())
			return Array.Empty<XcodeInfo>();

		var results = new List<XcodeInfo>();
		var paths = new List<string>();

		var selected = await GetSelectedXCodePathAsync(cancellationToken).ConfigureAwait(false);

		if (!string.IsNullOrEmpty(selected))
			paths.Add(selected);

		if (!cancellationToken.IsCancellationRequested)
		{
			var others = FindXCodeInstalls();

			if (others.Any())
				paths.AddRange(others);

			foreach (var p in paths.Distinct())
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				var info = GetXcodeInfo(p, selected == p);
				if (info != null)
					results.Add(info);
			}
		}

		return results;
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

	static IEnumerable<string> FindXCodeInstalls()
	{
		var paths = new List<string> {
			"/Applications/Xcode.app"
		};

		var others = Directory.GetDirectories("/Applications", "Xcode-*.app", SearchOption.TopDirectoryOnly);

		if (others.Length > 0)
			paths.AddRange(others);

		foreach (var p in paths)
		{
			var i = GetXcodeInfo(p, false)?.Path;
			if (i != null)
				yield return i;
		}
	}

	static Version ParseVersion(string path, string versionKey = "CFBundleShortVersionString")
	{
		var version = new Version();
		var plist = PropertyListParser.Parse(path);
		if (plist is NSDictionary dict)
		{
			if (dict.TryGetValue(versionKey, out var nsVersion)
				&& nsVersion is NSString nsStringVersion)
			{
				if (!Version.TryParse(nsStringVersion.Content, out version))
					version = new();
			}
		}
		return version;
	}

	static XcodeInfo? GetXcodeInfo(string path, bool selected)
	{
		var versionPlistFiles = new string[] {
			Path.Combine(path, "Contents", "version.plist"),
			Path.Combine(path, "Contents", "Info.plist")
		};
		
		foreach (var plistFile in versionPlistFiles)
		{
			if (File.Exists(plistFile))
			{
				var version = ParseVersion(plistFile);
				return new XcodeInfo(path, selected, version);
			}
		}

		return null;
	}
}
