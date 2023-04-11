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
			Client = new AppStoreConnectClient(
				new AppStoreConnectConfiguration(
					Environment.GetEnvironmentVariable("APP_STORE_CONNECT_KEY_ID") ?? "",
					Environment.GetEnvironmentVariable("APP_STORE_CONNECT_ISSUER_ID") ?? "",
					Environment.GetEnvironmentVariable("APP_STORE_CONNECT_PRIVATE_KEY") ?? ""));
		}


		readonly AppStoreConnectClient Client;

		[Fact]
		public async Task ListCertificates()
		{
			var certs = await Client.ListCertificatesAsync();
			Assert.NotNull(certs);
			Assert.NotEmpty(certs.Data);
		}
		
		[Fact]
		public async Task ListProfiles()
		{
			var profiles = await Client.ListProfilesAsync();
			Assert.NotNull(profiles);
			Assert.NotEmpty(profiles.Data);
		}
		
		[Fact]
		public async Task ListDevices()
		{
			var devices = await Client.ListDevicesAsync();
			Assert.NotNull(devices);
			Assert.NotEmpty(devices.Data);
		}
		
		[Fact]
		public async Task ListBundleIds()
		{
			var bundleIds = await Client.ListBundleIdsAsync();
			Assert.NotNull(bundleIds);
			Assert.NotEmpty(bundleIds.Data);
		}
		
		// [Fact]
		// public async Task CreateCertificate()
		// {
		// 	var cert = await Client.CreateCertificateAsync();
		// 	
		// 	Assert.NotNull(cert?.Data?.Attributes?.CertificateContent);
		// }
	}
}
