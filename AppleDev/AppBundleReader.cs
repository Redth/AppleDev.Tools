using Claunia.PropertyList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleDev;

public class AppBundleReader
{
	public AppBundleReader(string appFilename)
	{
		if (!File.Exists(appFilename) && !Directory.Exists(appFilename))
			throw new ArgumentNullException(nameof(appFilename));

		if (appFilename.EndsWith(".app", StringComparison.InvariantCultureIgnoreCase))
		{
			// iOS is in root of .app
			InfoPlistFile = Path.Combine(appFilename.TrimEnd('/') + "/", "Info.plist");

			// If not there, check for mac pattern inside Contents/Info.plist
			if (!File.Exists(InfoPlistFile))
				InfoPlistFile = Path.Combine(appFilename.TrimEnd('/') + "/", "Contents", "Info.plist");
		}
	}

	public readonly string? InfoPlistFile;

	public AppInfoPlist ReadInfoPlist()
	{
		if (!File.Exists(InfoPlistFile))
			throw new Exception($"Could not find Info.plist in {InfoPlistFile}");

		return new AppInfoPlist(InfoPlistFile);
	}
}

public class AppInfoPlist
{
	public AppInfoPlist(string infoPlistFile)
	{
		Filename = infoPlistFile;
		Root = PropertyListParser.Parse(Filename) as NSDictionary;
	}

	public readonly string Filename;

	public readonly NSDictionary? Root;

	public string? CFBundleVersion
		=> Root?.ObjectForKey("CFBundleVersion")?.ToString();

	public string? CFBundleShortVersionString
		=> Root?.ObjectForKey("CFBundleShortVersionString")?.ToString();

	public string? CFBundleDisplayName
		=> Root?.ObjectForKey("CFBundleDisplayName")?.ToString();

	public string? CFBundleName
		=> Root?.ObjectForKey("CFBundleName")?.ToString();

	public string? CFBundleIdentifier
		=> Root?.ObjectForKey("CFBundleIdentifier")?.ToString();

	public string? DTPlatformName
		=> Root?.ObjectForKey("DTPlatformName")?.ToString();

	public string? DTPlatformVersion
		=> Root?.ObjectForKey("DTPlatformVersion")?.ToString();

	public string? DTSDKName
		=> Root?.ObjectForKey("DTSDKName")?.ToString();
}
