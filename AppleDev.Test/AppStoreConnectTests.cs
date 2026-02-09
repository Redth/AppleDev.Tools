using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AppleAppStoreConnect;

namespace AppleDev.Test
{
	public class AppStoreConnectTests
	{
		static AppStoreConnectTests()
		{
			// Load .env file if it exists in the project root
			DotEnvLoader.Load();
		}

		public AppStoreConnectTests()
		{
			var keyId = Environment.GetEnvironmentVariable("APP_STORE_CONNECT_KEY_ID");
			var issuerId = Environment.GetEnvironmentVariable("APP_STORE_CONNECT_ISSUER_ID");
			var privateKey = Environment.GetEnvironmentVariable("APP_STORE_CONNECT_PRIVATE_KEY");

			// Skip tests if credentials are not configured
			if (string.IsNullOrWhiteSpace(keyId) || 
			    string.IsNullOrWhiteSpace(issuerId) || 
			    string.IsNullOrWhiteSpace(privateKey))
			{
				HasCredentials = false;
				return;
			}

			HasCredentials = true;
			Client = new AppStoreConnectClient(
				new AppStoreConnectConfiguration(keyId, issuerId, privateKey));
		}

		readonly AppStoreConnectClient? Client;
		readonly bool HasCredentials;

	// ===== CERTIFICATE TESTS =====

	[SkippableFact]
	public async Task ListCertificates()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var certs = await Client!.ListCertificatesAsync();
		Assert.NotNull(certs);
		Assert.NotEmpty(certs.Data);
	}

	[SkippableFact]
	public async Task ListCertificates_WithLimit()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var certs = await Client!.ListCertificatesAsync(limit: 5);
		Assert.NotNull(certs);
		Assert.NotNull(certs.Data);
		Assert.True(certs.Data.Count() <= 5);
	}

	[SkippableFact]
	public async Task ListCertificates_FilterByCertificateType()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var certs = await Client!.ListCertificatesAsync(
			filterCertificateType: new[] { CertificateType.IOS_DISTRIBUTION });
		Assert.NotNull(certs);
		Assert.NotNull(certs.Data);

		if (certs.Data.Any())
		{
			Assert.All(certs.Data, cert =>
				Assert.Equal("IOS_DISTRIBUTION", cert.Attributes.CertificateTypeValue));
		}
	}

	[SkippableFact]
	public async Task ListCertificates_WithSort()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var certs = await Client!.ListCertificatesAsync(sort: "-id");
		Assert.NotNull(certs);
		Assert.NotNull(certs.Data);
	}

	// ===== DEVICE TESTS =====

	[SkippableFact]
	public async Task ListDevices()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var devices = await Client!.ListDevicesAsync();
		Assert.NotNull(devices);
		Assert.NotEmpty(devices.Data);
	}

	[SkippableFact]
	public async Task ListDevices_WithLimit()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var devices = await Client!.ListDevicesAsync(limit: 10);
		Assert.NotNull(devices);
		Assert.NotNull(devices.Data);
		Assert.True(devices.Data.Count() <= 10);
	}

	[SkippableFact]
	public async Task ListDevices_FilterByPlatform()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var devices = await Client!.ListDevicesAsync(
			filterPlatform: new[] { Platform.IOS });
		Assert.NotNull(devices);
		Assert.NotNull(devices.Data);

		if (devices.Data.Any())
		{
			Assert.All(devices.Data, device =>
				Assert.Equal("IOS", device.Attributes.PlatformValue));
		}
	}

	[SkippableFact]
	public async Task ListDevices_FilterByStatus()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var devices = await Client!.ListDevicesAsync(
			filterStatus: new[] { DeviceStatus.ENABLED });
		Assert.NotNull(devices);
		Assert.NotNull(devices.Data);
	}

	[SkippableFact]
	public async Task ListDevices_WithSort()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var devices = await Client!.ListDevicesAsync(sort: "name");
		Assert.NotNull(devices);
		Assert.NotNull(devices.Data);
	}

	// ===== BUNDLE ID TESTS =====

	[SkippableFact]
	public async Task ListBundleIds()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var bundleIds = await Client!.ListBundleIdsAsync();
		Assert.NotNull(bundleIds);
		Assert.NotEmpty(bundleIds.Data);
	}

	[SkippableFact]
	public async Task ListBundleIds_WithLimit()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var bundleIds = await Client!.ListBundleIdsAsync(limit: 5);
		Assert.NotNull(bundleIds);
		Assert.NotNull(bundleIds.Data);
		Assert.True(bundleIds.Data.Count() <= 5);
	}

	[SkippableFact]
	public async Task ListBundleIds_FilterByPlatform()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var bundleIds = await Client!.ListBundleIdsAsync(
			filterPlatform: new[] { Platform.IOS });
		Assert.NotNull(bundleIds);
		Assert.NotNull(bundleIds.Data);

		if (bundleIds.Data.Any())
		{
			// Output diagnostic information about what we received
			var nonIosBundles = bundleIds.Data
				.Where(b => b.Attributes.PlatformValue != "IOS")
				.Select(b => $"{b.Attributes.Name}: {b.Attributes.PlatformValue}")
				.ToList();
			
			if (nonIosBundles.Any())
			{
				var message = $"Found {nonIosBundles.Count} non-IOS bundle(s): {string.Join(", ", nonIosBundles)}";
				throw new Exception(message);
			}

			Assert.All(bundleIds.Data, bundle =>
				Assert.Equal("IOS", bundle.Attributes.PlatformValue));
		}
	}

	[SkippableFact]
	public async Task ListBundleIds_FilterByIdentifier()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get first bundle ID to use as filter
		var allBundleIds = await Client!.ListBundleIdsAsync(limit: 1);
		Skip.If(!allBundleIds.Data.Any(), "No bundle IDs available for filtering test");

		var firstIdentifier = allBundleIds.Data.First().Attributes.Identifier;

		var filteredBundleIds = await Client!.ListBundleIdsAsync(
			filterIdentifier: new[] { firstIdentifier });
		Assert.NotNull(filteredBundleIds);
		Assert.NotEmpty(filteredBundleIds.Data);
		Assert.All(filteredBundleIds.Data, bundle =>
			Assert.StartsWith(firstIdentifier, bundle.Attributes.Identifier));
	}

	[SkippableFact]
	public async Task ListBundleIds_WithSort()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var bundleIds = await Client!.ListBundleIdsAsync(sort: "name");
		Assert.NotNull(bundleIds);
		Assert.NotNull(bundleIds.Data);
	}

	// ===== PROFILE TESTS =====

	[SkippableFact]
	public async Task ListProfiles()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var profiles = await Client!.ListProfilesAsync();
		Assert.NotNull(profiles);
		Assert.NotEmpty(profiles.Data);
	}

	[SkippableFact]
	public async Task ListProfiles_WithLimit()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var profiles = await Client!.ListProfilesAsync(limit: 5);
		Assert.NotNull(profiles);
		Assert.NotNull(profiles.Data);
		Assert.True(profiles.Data.Count() <= 5);
	}

	[SkippableFact]
	public async Task ListProfiles_FilterByProfileState()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var profiles = await Client!.ListProfilesAsync(
			filterProfileState: new[] { ProfileState.ACTIVE });
		Assert.NotNull(profiles);
		Assert.NotNull(profiles.Data);

		if (profiles.Data.Any())
		{
			Assert.All(profiles.Data, profile =>
				Assert.Equal("ACTIVE", profile.Attributes.ProfileStateValue));
		}
	}

	[SkippableFact]
	public async Task ListProfiles_FilterByProfileType()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var profiles = await Client!.ListProfilesAsync(
			filterProfileType: new[] { ProfileType.IOS_APP_DEVELOPMENT });
		Assert.NotNull(profiles);
		Assert.NotNull(profiles.Data);

		if (profiles.Data.Any())
		{
			Assert.All(profiles.Data, profile =>
				Assert.Equal("IOS_APP_DEVELOPMENT", profile.Attributes.ProfileTypeValue));
		}
	}

	[SkippableFact]
	public async Task ListProfiles_WithInclude()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var profiles = await Client!.ListProfilesAsync(
			include: "bundleId,devices",
			limit: 5);
		Assert.NotNull(profiles);
		Assert.NotNull(profiles.Data);

		// Check that relationships are present
		if (profiles.Data.Any())
		{
			var firstProfile = profiles.Data.First();
			Assert.NotNull(firstProfile.Relationships);
		}
	}

	[SkippableFact]
	public async Task ListProfiles_FilterByName()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get first profile to use as filter
		var allProfiles = await Client!.ListProfilesAsync(limit: 1);
		Skip.If(!allProfiles.Data.Any(), "No profiles available for filtering test");

		var firstName = allProfiles.Data.First().Attributes.Name;

		var filteredProfiles = await Client!.ListProfilesAsync(
			filterName: new[] { firstName });
		Assert.NotNull(filteredProfiles);
		Assert.NotEmpty(filteredProfiles.Data);
		Assert.All(filteredProfiles.Data, profile =>
			Assert.Equal(firstName, profile.Attributes.Name));
	}

	[SkippableFact]
	public async Task ListProfiles_WithSort()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		var profiles = await Client!.ListProfilesAsync(sort: "name");
		Assert.NotNull(profiles);
		Assert.NotNull(profiles.Data);
	}

	[SkippableFact]
	public async Task ListProfiles_WithDevices_FetchesDeviceInfo()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// First, get profiles with device relationships
		var profiles = await Client!.ListProfilesAsync(
			include: "bundleId,devices",
			limit: 10);
		Assert.NotNull(profiles);
		Assert.NotNull(profiles.Data);

		// Find a profile that has devices
		var profileWithDevices = profiles.Data
			.FirstOrDefault(p => p.Relationships.TryGetValue("devices", out var devRel) &&
			                     devRel?.Data != null &&
			                     devRel.Data.Any());

		Skip.If(profileWithDevices == null, "No profiles with devices found");

		// Collect device IDs from the profile
		var deviceIds = new HashSet<string>();
		if (profileWithDevices.Relationships.TryGetValue("devices", out var devicesRel) && devicesRel?.Data != null)
		{
			foreach (var device in devicesRel.Data)
			{
				deviceIds.Add(device.Id);
			}
		}

		Assert.NotEmpty(deviceIds);

		// Fetch devices by ID
		var devicesResponse = await Client!.ListDevicesAsync(
			filterId: deviceIds.ToArray(),
			limit: 200);

		Assert.NotNull(devicesResponse);
		Assert.NotNull(devicesResponse.Data);
		Assert.NotEmpty(devicesResponse.Data);

		// Verify each device has expected attributes
		foreach (var device in devicesResponse.Data)
		{
			Assert.NotNull(device.Attributes);
			Assert.NotNull(device.Attributes.Name);
			Assert.NotNull(device.Attributes.Udid);
			Assert.NotNull(device.Attributes.PlatformValue);
		}
	}

	[SkippableFact]
	public async Task ListProfiles_ProfileData_IsAvailable()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a profile
		var profiles = await Client!.ListProfilesAsync(limit: 1);
		Assert.NotNull(profiles);
		Assert.NotEmpty(profiles.Data);

		var profile = profiles.Data.First();
		Assert.NotNull(profile.Attributes);
		Assert.NotNull(profile.Attributes.ProfileContent);
		Assert.NotEmpty(profile.Attributes.ProfileContent);

		// Verify it's valid base64
		var profileBytes = Convert.FromBase64String(profile.Attributes.ProfileContent);
		Assert.NotEmpty(profileBytes);
	}

	// ===== INTEGRATION TESTS =====

	[SkippableFact]
	public async Task ListDevices_FilterById()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a device ID first
		var allDevices = await Client!.ListDevicesAsync(limit: 1);
		Skip.If(!allDevices.Data.Any(), "No devices available for filtering test");

		var deviceId = allDevices.Data.First().Id;

		// Filter by that ID
		var filteredDevices = await Client!.ListDevicesAsync(
			filterId: new[] { deviceId });
		Assert.NotNull(filteredDevices);
		Assert.Single(filteredDevices.Data);
		Assert.Equal(deviceId, filteredDevices.Data.First().Id);
	}

	[SkippableFact]
	public async Task ListCertificates_FilterById()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a certificate ID first
		var allCerts = await Client!.ListCertificatesAsync(limit: 1);
		Skip.If(!allCerts.Data.Any(), "No certificates available for filtering test");

		var certId = allCerts.Data.First().Id;

		// Filter by that ID
		var filteredCerts = await Client!.ListCertificatesAsync(
			filterId: new[] { certId });
		Assert.NotNull(filteredCerts);
		Assert.Single(filteredCerts.Data);
		Assert.Equal(certId, filteredCerts.Data.First().Id);
	}

	[SkippableFact]
	public async Task ListProfiles_FilterById()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a profile ID first
		var allProfiles = await Client!.ListProfilesAsync(limit: 1);
		Skip.If(!allProfiles.Data.Any(), "No profiles available for filtering test");

		var profileId = allProfiles.Data.First().Id;

		// Filter by that ID
		var filteredProfiles = await Client!.ListProfilesAsync(
			filterId: new[] { profileId });
		Assert.NotNull(filteredProfiles);
		Assert.Single(filteredProfiles.Data);
		Assert.Equal(profileId, filteredProfiles.Data.First().Id);
	}

	[SkippableFact]
	public async Task ListBundleIds_FilterById()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a bundle ID first
		var allBundleIds = await Client!.ListBundleIdsAsync(limit: 1);
		Skip.If(!allBundleIds.Data.Any(), "No bundle IDs available for filtering test");

		var bundleId = allBundleIds.Data.First().Id;

		// Filter by that ID
		var filteredBundleIds = await Client!.ListBundleIdsAsync(
			filterId: new[] { bundleId });
		Assert.NotNull(filteredBundleIds);
		Assert.Single(filteredBundleIds.Data);
		Assert.Equal(bundleId, filteredBundleIds.Data.First().Id);
	}

	// ===== BUNDLE ID CAPABILITY TESTS =====

	[SkippableFact]
	public async Task ListBundleIdCapabilities()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a bundle ID first
		var bundleIds = await Client!.ListBundleIdsAsync(limit: 1);
		Skip.If(!bundleIds.Data.Any(), "No bundle IDs available for capability test");

		var bundleIdResourceId = bundleIds.Data.First().Id;

		// List capabilities for that bundle ID
		var capabilities = await Client!.ListBundleIdCapabilitiesAsync(bundleIdResourceId);
		Assert.NotNull(capabilities);
		Assert.NotNull(capabilities.Data);
		// Note: A bundle ID may have no capabilities, so we don't assert NotEmpty
	}

	[SkippableFact]
	public async Task ListBundleIdCapabilities_ReturnsCapabilityDetails()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");

		// Get a bundle ID first
		var bundleIds = await Client!.ListBundleIdsAsync(limit: 1);
		Skip.If(!bundleIds.Data.Any(), "No bundle IDs available for capability test");

		var bundleIdResourceId = bundleIds.Data.First().Id;

		// List capabilities
		var capabilities = await Client!.ListBundleIdCapabilitiesAsync(bundleIdResourceId);
		Assert.NotNull(capabilities);
		Assert.NotNull(capabilities.Data);
		
		// If there are capabilities, verify they have the expected structure
		if (capabilities.Data.Any())
		{
			var capability = capabilities.Data.First();
			Assert.NotNull(capability.Id);
			Assert.NotNull(capability.Type);
			Assert.Equal(BundleIdCapability.TYPE, capability.Type);
			Assert.NotNull(capability.Attributes);
			Assert.NotEmpty(capability.Attributes.CapabilityTypeValue);
		}
	}

	[Fact]
	public void GetAvailableCapabilityTypes_ReturnsAllTypes()
	{
		var types = AppStoreConnectClient.GetAvailableCapabilityTypes();
		Assert.NotNull(types);
		Assert.NotEmpty(types);
		Assert.DoesNotContain(CapabilityType.Unknown, types);
		Assert.Contains(CapabilityType.PUSH_NOTIFICATIONS, types);
		Assert.Contains(CapabilityType.ICLOUD, types);
		Assert.Contains(CapabilityType.GAME_CENTER, types);
	}

	[Fact]
	public void CapabilityType_EnumHasExpectedValues()
	{
		// Verify common capability types exist
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.PUSH_NOTIFICATIONS));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.ICLOUD));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.GAME_CENTER));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.IN_APP_PURCHASE));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.APP_GROUPS));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.APPLE_PAY));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.ASSOCIATED_DOMAINS));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.HEALTHKIT));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.HOMEKIT));
		Assert.True(Enum.IsDefined(typeof(CapabilityType), CapabilityType.SIRIKIT));
	}

	[Fact]
	public void BundleIdCapabilityAttributes_CapabilityTypeParsesCorrectly()
	{
		var attrs = new BundleIdCapabilityAttributes
		{
			CapabilityTypeValue = "PUSH_NOTIFICATIONS"
		};

		Assert.Equal(CapabilityType.PUSH_NOTIFICATIONS, attrs.CapabilityType);
	}

	[Fact]
	public void BundleIdCapabilityAttributes_UnknownCapabilityType_ReturnsUnknown()
	{
		var attrs = new BundleIdCapabilityAttributes
		{
			CapabilityTypeValue = "SOME_FUTURE_CAPABILITY"
		};

		Assert.Equal(CapabilityType.Unknown, attrs.CapabilityType);
	}

	[Fact]
	public void BundleIdCapabilityAttributes_SetCapabilityType_UpdatesValue()
	{
		var attrs = new BundleIdCapabilityAttributes();
		attrs.CapabilityType = CapabilityType.GAME_CENTER;

		Assert.Equal("GAME_CENTER", attrs.CapabilityTypeValue);
	}

	// ===== CERTIFICATE PLATFORM TESTS =====

	[Fact]
	public void CertificateAttributes_Platform_ReturnsNullWhenEmpty()
	{
		var attrs = new CertificateAttributes
		{
			PlatformValue = string.Empty
		};

		Assert.Null(attrs.Platform);
	}

	[Fact]
	public void CertificateAttributes_Platform_ReturnsNullWhenNull()
	{
		var attrs = new CertificateAttributes
		{
			PlatformValue = null!
		};

		Assert.Null(attrs.Platform);
	}

	[Fact]
	public void CertificateAttributes_Platform_ReturnsValueWhenValid()
	{
		var attrs = new CertificateAttributes
		{
			PlatformValue = "MAC_OS"
		};

		Assert.Equal(Platform.MAC_OS, attrs.Platform);
	}

	[Fact]
	public void CertificateAttributes_Platform_ReturnsUnknownWhenUnrecognized()
	{
		var attrs = new CertificateAttributes
		{
			PlatformValue = "SOME_FUTURE_PLATFORM"
		};

		Assert.Equal(Platform.Unknown, attrs.Platform);
	}

	[Fact]
	public void CertificateAttributes_SetPlatform_UpdatesValue()
	{
		var attrs = new CertificateAttributes();
		attrs.Platform = Platform.IOS;

		Assert.Equal("IOS", attrs.PlatformValue);
	}

	[Fact]
	public void CertificateAttributes_SetPlatformNull_ClearsValue()
	{
		var attrs = new CertificateAttributes
		{
			PlatformValue = "IOS"
		};
		attrs.Platform = null;

		Assert.Equal(string.Empty, attrs.PlatformValue);
	}

	// ===== PROFILE PLATFORM TESTS =====

	[Fact]
	public void ProfileAttributes_Platform_ReturnsNullWhenEmpty()
	{
		var attrs = new ProfileAttributes
		{
			PlatformValue = string.Empty
		};

		Assert.Null(attrs.Platform);
	}

	[Fact]
	public void ProfileAttributes_Platform_ReturnsValueWhenValid()
	{
		var attrs = new ProfileAttributes
		{
			PlatformValue = "IOS"
		};

		Assert.Equal(Platform.IOS, attrs.Platform);
	}

	[Fact]
	public void ProfileAttributes_Platform_ReturnsUnknownWhenUnrecognized()
	{
		var attrs = new ProfileAttributes
		{
			PlatformValue = "SOME_FUTURE_PLATFORM"
		};

		Assert.Equal(Platform.Unknown, attrs.Platform);
	}

	[Fact]
	public void ProfileAttributes_Platform_ReturnsNullWhenNull()
	{
		var attrs = new ProfileAttributes
		{
			PlatformValue = null!
		};

		Assert.Null(attrs.Platform);
	}

	[Fact]
	public void ProfileAttributes_SetPlatform_UpdatesValue()
	{
		var attrs = new ProfileAttributes();
		attrs.Platform = Platform.MAC_OS;

		Assert.Equal("MAC_OS", attrs.PlatformValue);
	}

	[Fact]
	public void ProfileAttributes_SetPlatformNull_ClearsValue()
	{
		var attrs = new ProfileAttributes
		{
			PlatformValue = "IOS"
		};
		attrs.Platform = null;

		Assert.Equal(string.Empty, attrs.PlatformValue);
	}

	// ===== BUNDLE ID PLATFORM TESTS =====

	[Fact]
	public void BundleIdAttributes_Platform_ReturnsNullWhenEmpty()
	{
		var attrs = new BundleIdAttributes
		{
			PlatformValue = string.Empty
		};

		Assert.Null(attrs.Platform);
	}

	[Fact]
	public void BundleIdAttributes_Platform_ReturnsNullWhenNull()
	{
		var attrs = new BundleIdAttributes
		{
			PlatformValue = null!
		};

		Assert.Null(attrs.Platform);
	}

	[Fact]
	public void BundleIdAttributes_Platform_ReturnsValueWhenValid()
	{
		var attrs = new BundleIdAttributes
		{
			PlatformValue = "IOS"
		};

		Assert.Equal(Platform.IOS, attrs.Platform);
	}

	[Fact]
	public void BundleIdAttributes_Platform_ReturnsUniversal()
	{
		var attrs = new BundleIdAttributes
		{
			PlatformValue = "UNIVERSAL"
		};

		Assert.Equal(Platform.UNIVERSAL, attrs.Platform);
	}

	[Fact]
	public void BundleIdAttributes_Platform_ReturnsUnknownWhenUnrecognized()
	{
		var attrs = new BundleIdAttributes
		{
			PlatformValue = "SOME_FUTURE_PLATFORM"
		};

		Assert.Equal(Platform.Unknown, attrs.Platform);
	}

	[Fact]
	public void BundleIdAttributes_SetPlatform_UpdatesValue()
	{
		var attrs = new BundleIdAttributes();
		attrs.Platform = Platform.UNIVERSAL;

		Assert.Equal("UNIVERSAL", attrs.PlatformValue);
	}

	[Fact]
	public void BundleIdAttributes_SetPlatformNull_ClearsValue()
	{
		var attrs = new BundleIdAttributes
		{
			PlatformValue = "IOS"
		};
		attrs.Platform = null;

		Assert.Equal(string.Empty, attrs.PlatformValue);
	}

	// ===== CERTIFICATE TYPE SETTER TESTS =====

	[Fact]
	public void CertificateAttributes_SetCertificateType_UpdatesValue()
	{
		var attrs = new CertificateAttributes();
		attrs.CertificateType = CertificateType.DEVELOPER_ID_APPLICATION;

		Assert.Equal("DEVELOPER_ID_APPLICATION", attrs.CertificateTypeValue);
	}

	[Fact]
	public void CertificateAttributes_SetCertificateType_RoundTrips()
	{
		var attrs = new CertificateAttributes();
		attrs.CertificateType = CertificateType.IOS_DISTRIBUTION;

		Assert.Equal(CertificateType.IOS_DISTRIBUTION, attrs.CertificateType);
	}

	[Fact]
	public void CreateCertificateRequestAttributes_EnumConstructor_SetsCertificateTypeValue()
	{
		var attrs = new CreateCertificateRequestAttributes("csr-content", CertificateType.DEVELOPER_ID_APPLICATION);

		Assert.Equal("DEVELOPER_ID_APPLICATION", attrs.CertificateTypeValue);
	}

	[Fact]
	public void CreateCertificateRequestAttributes_StringConstructor_SetsCertificateTypeValue()
	{
		var attrs = new CreateCertificateRequestAttributes("csr-content", "MAC_APP_DISTRIBUTION");

		Assert.Equal("MAC_APP_DISTRIBUTION", attrs.CertificateTypeValue);
	}
	}
}
