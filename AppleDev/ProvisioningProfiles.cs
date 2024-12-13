using Claunia.PropertyList;
using System.Security.Cryptography.Pkcs;
using Newtonsoft.Json;

namespace AppleDev;


public static class ProvisioningProfiles
{
	public static async Task<DirectoryInfo> GetDirectory(Version? xcodeVersion = default)
	{
		if (OperatingSystem.IsMacOS())
		{
			if (xcodeVersion is null)
			{
				var xcode = new Xcode();
				var xcodeInfo = await xcode.LocateBestAsync().ConfigureAwait(false);
				// If we can't find xcode, let's assume to use the newest path
				xcodeVersion = xcodeInfo?.Version ?? new Version(16, 0);
			}

			if (xcodeVersion >= new Version(16, 0))
			{
				return new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Developer", "Xcode", "UserData", "Provisioning Profiles"));
			}
			else
			{
				return new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "MobileDevice", "Provisioning Profiles"));
			}
		}

		return new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xamarin", "iOS", "Provisioning", "Profiles"));
	}

	public static async Task<IReadOnlyList<ProvisioningProfileInfo>> ListProfilesAsync(
		DirectoryInfo? directory = default)
	{
		var dir = directory ?? await GetDirectory().ConfigureAwait(false);

		if (!dir.Exists)
			return Array.Empty<ProvisioningProfileInfo>();

		var results = new List<ProvisioningProfileInfo>();

		foreach (var file in dir.EnumerateFiles("*.mobileprovision").Concat(dir.EnumerateFiles("*.provisionprofile")))
		{
			var profileData = await File.ReadAllBytesAsync(file.FullName).ConfigureAwait(false);
			var profile = await ParseAsync(profileData).ConfigureAwait(false);
			results.Add(profile);
		}

		return results;	
	}
	
	
	public static async Task InstallProfileAsync(byte[] profileData, DirectoryInfo? directory = default)
	{
		var dir = directory ?? await GetDirectory().ConfigureAwait(false);

		// Create the folder if it doesn't exist
		if (!dir.Exists)
			dir.Create();
		
		var profile = await ParseAsync(profileData);
		
		if (profile is null)
			throw new InvalidOperationException("Invalid provisioning profile");

		
		var extension = profile.Platform.Any(p => p == "MacOS")
				? "provisionprofile"
				: "mobileprovision";

		var profileFilename = Path.Combine(directory.FullName, $"{profile.Uuid}.{extension}");

			// Write the file
		await File.WriteAllBytesAsync(profileFilename, profileData).ConfigureAwait(false);
	}

	public static async Task<ProvisioningProfileInfo> ParseAsync(byte[] profileData)
	{
		// Create a SignedCms object to decode the S/MIME structure
		var signedCms = new SignedCms();

		// Decode the DER-encoded data
		signedCms.Decode(profileData);

		var profile = PropertyListParser.Parse(signedCms.ContentInfo.Content) as NSDictionary;

		if (profile is null)
			throw new InvalidOperationException("Invalid provisioning profile");

		var info = new ProvisioningProfileInfo();

		if (profile.TryGetValue("AppIDName", out var appIdName) && appIdName is NSString appIdNameString)
			info.AppIdName = appIdNameString.Content;

		if (profile.TryGetValue("ApplicationIdentifierPrefix", out var appIdPrefix) &&
		    appIdPrefix is NSArray appIdPrefixArray)
			info.ApplicationIdentifierPrefix = NSArrayToString(appIdPrefixArray);
		
		if (profile.TryGetValue("Name", out var name) && name is NSString nameString)
			info.Name = nameString.Content;
		
		if (profile.TryGetValue("ProvisionedDevices", out var provDevices) && provDevices is NSArray provDevicesArray)
			info.ProvisionedDevices = NSArrayToString(provDevicesArray);
		
		if (profile.TryGetValue("CreationDate", out var creationDate) && creationDate is NSDate creationDateValue)
			info.CreationDate = creationDateValue.Date;

		if (profile.TryGetValue("Platform", out var platform) && platform is NSArray platformArray)
			info.Platform = NSArrayToString(platformArray);

		if (profile.TryGetValue("IsXcodeManaged", out var isXcodeManaged) && isXcodeManaged is NSNumber isXcodeManagedValue)
			info.IsXcodeManaged = isXcodeManagedValue.ToBool();

		if (profile.TryGetValue("DeveloperCertificates", out var devCerts) && devCerts is NSArray devCertsArray)
		{
			var devCertsDatas = new List<byte[]>();
			foreach (var dc in devCertsArray)
			{
				if (dc is NSData devCertData)
				{
					devCertsDatas.Add(devCertData.Bytes);
				}
			}
			info.DeveloperCertificates = devCertsDatas.ToArray();
		}
		
		if (profile.TryGetValue("DER-Encoded-Profile", out var derEncProfile) && derEncProfile is NSData derEncProfileData)
			info.ProfileData = derEncProfileData.Bytes;
		
		if (profile.TryGetValue("Entitlements", out var entitlements) && entitlements is NSDictionary entitlementsDict)
		{
			var entitlementsResult = new Dictionary<string, object>();
			
			foreach (var ek in entitlementsDict.Keys)
			{
				var ev = entitlementsDict.Get(ek);
				var obj = NSObjectToObject(ev);
				if (obj is not null)
					entitlementsResult.Add(ek, obj);
			}
			info.Entitlements = entitlementsResult;
		}
		
		if (profile.TryGetValue("ExpirationDate", out var expirationDate) && expirationDate is NSDate expirationDateValue)
			info.ExpirationDate = expirationDateValue.Date;

		if (profile.TryGetValue("TeamName", out var teamName) && teamName is NSString teamNameString)
			info.TeamName = teamNameString.Content;

		if (profile.TryGetValue("TeamIdentifier", out var teamIdentifier) && teamIdentifier is NSArray teamIdentifierArray)
			info.TeamIdentifier = NSArrayToString(teamIdentifierArray);

		if (profile.TryGetValue("TimeToLive", out var timeToLive) && timeToLive is NSNumber timeToLiveValue)
			info.TimeToLive = timeToLiveValue.ToLong();

		if (profile.TryGetValue("UUID", out var uuid) && uuid is NSString uuidString)
			info.Uuid = uuidString.Content;
		
		if (profile.TryGetValue("Version", out var version) && version is NSNumber versionValue)
			info.Version = versionValue.ToLong();

		return info;
	}

	static object? NSObjectToObject(NSObject? ns)
	{
		if (ns is null)
			return null;
		
		if (ns is NSString evStr)
			return evStr.Content;
		else if (ns is NSNumber evNum)
		{
			if (evNum.isBoolean())
				return evNum.ToBool();
			else if (evNum.isInteger())
				return evNum.ToLong();
			else if (evNum.isReal())
				return evNum.ToDouble();
			else
				return evNum.ToString();
		}
		else if (ns is NSArray nsArr)
		{
			var results = new List<object>();
			foreach (var nsArrObj in nsArr)
			{
				var obj = NSObjectToObject(nsArrObj);
				if (obj is not null)
					results.Add(obj);
			}

			return (results.ToArray());
		}
		else if (ns is NSDictionary nsDict)
		{
			var results = new Dictionary<string, object?>();
			foreach (var key in nsDict.Keys)
			{
				var value = nsDict.Get(key);
				results.Add(key, NSObjectToObject(value));
			}

			return (IDictionary<string, object?>)results;
		}
		else
		{
			return ns.ToString();
		}
	}
	
	static string[] NSArrayToString(NSArray array)
	{
		var results = new List<string>();
		foreach (var obj in array)
		{
			var s = obj.ToString();
			if (!string.IsNullOrEmpty(s))
				results.Add(s);
		}

		return results.ToArray();
	}
}

public class ProvisioningProfileInfo
{
	public string? AppIdName { get; set; }

	public string? Name { get;set; }

	public string[] ApplicationIdentifierPrefix { get; set; } = [];

	public DateTimeOffset CreationDate { get; set; }

	public string[] Platform {get;set;} = [];

	public bool IsXcodeManaged {get;set; } = false;

	public DateTimeOffset ExpirationDate { get; set; }

	public string? TeamName { get; set; }

	public string[] TeamIdentifier { get; set; } = [];

	public long TimeToLive { get; set; }

	public string? Uuid { get; set; }

	public long? Version { get; set; }

	public byte[]? ProfileData { get; set; }

	public string[] ProvisionedDevices { get; set; } = [];

	public byte[][] DeveloperCertificates { get; set; } = [];

	public IReadOnlyDictionary<string, object> Entitlements { get; set; } = new Dictionary<string, object>();
}