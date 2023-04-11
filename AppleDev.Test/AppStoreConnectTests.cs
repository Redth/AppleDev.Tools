using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppleDev.Test
{
	public class AppStoreConnectTests
	{
		public AppStoreConnectTests()
		{
		}


		[Fact]
		public async Task CreateCertificate()
		{
			var config = new AppStoreConnect.Client.AppStoreConnectConfiguration(
				"",
				"",
				"");

			var api = new AppStoreConnect.Api.CertificatesApi(config);

			using var rsa = RSA.Create(2048);

			var certificateRequest = new CertificateRequest(
				$"CN={Environment.MachineName}",
				rsa,
				HashAlgorithmName.SHA256,
				RSASignaturePadding.Pkcs1);

			var pemr = certificateRequest.CreateSigningRequestPem();

			var response = await api.CertificatesCreateInstanceAsync(new AppStoreConnect.Model.CertificateCreateRequest(new AppStoreConnect.Model.CertificateCreateRequestData(AppStoreConnect.Model.CertificateCreateRequestData.TypeEnum.Certificates,
				new AppStoreConnect.Model.CertificateCreateRequestDataAttributes(
					pemr,
					AppStoreConnect.Model.CertificateType.DEVELOPMENT)))).ConfigureAwait(false);

			var exp = response.Data.Attributes.ExpirationDate;

			Assert.True(exp > DateTimeOffset.Now);
		}

		[Fact]
		public async Task ParseCertResp()
		{
			var content = "";

			var data = Convert.FromBase64String(content);

			var cert = new X509Certificate2(data);

			Assert.NotEmpty(cert.FriendlyName);
		}
	}
}
