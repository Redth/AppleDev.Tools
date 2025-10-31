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

	[SkippableFact]
	public async Task ListCertificates()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		
		var certs = await Client!.ListCertificatesAsync();
		Assert.NotNull(certs);
		Assert.NotEmpty(certs.Data);
	}
	
	[SkippableFact]
	public async Task ListProfiles()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		
		var profiles = await Client!.ListProfilesAsync();
		Assert.NotNull(profiles);
		Assert.NotEmpty(profiles.Data);
	}
	
	[SkippableFact]
	public async Task ListDevices()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		
		var devices = await Client!.ListDevicesAsync();
		Assert.NotNull(devices);
		Assert.NotEmpty(devices.Data);
	}
	
	[SkippableFact]
	public async Task ListBundleIds()
	{
		Skip.IfNot(HasCredentials, "App Store Connect credentials not configured");
		
		var bundleIds = await Client!.ListBundleIdsAsync();
		Assert.NotNull(bundleIds);
		Assert.NotEmpty(bundleIds.Data);
	}		// [Fact]
		// public async Task CreateCertificate()
		// {
		// 	var cert = await Client.CreateCertificateAsync();
		// 	
		// 	Assert.NotNull(cert?.Data?.Attributes?.CertificateContent);
		// }
	}
}
