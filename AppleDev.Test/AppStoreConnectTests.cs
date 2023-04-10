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
				"V3D6LGN2F4",
				"69a6de6f-1420-47e3-e053-5b8c7c11a4d1",
				"MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgMn6GkPE2XUtampp0i3ZLqagTK+s3mcmSP9D8YIv2bgugCgYIKoZIzj0DAQehRANCAARgTx3gBT3+StSiqq6FRsccKg+G4zl8VsJqxGUbMD6domE2bC/7p5b+WOTEYYwlTRyoAG1VEvGDkga/6GMFJ631");

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
			var content = "MIIFyjCCBLKgAwIBAgIQZ13QxlgzSibU3HoL+SLMSjANBgkqhkiG9w0BAQsFADB1MUQwQgYDVQQDDDtBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9ucyBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTELMAkGA1UECwwCRzMxEzARBgNVBAoMCkFwcGxlIEluYy4xCzAJBgNVBAYTAlVTMB4XDTIzMDQxMDE5MDEyMVoXDTI0MDQwOTE5MDEyMFowgZAxGjAYBgoJkiaJk/IsZAEBDApWM0Q2TEdOMkY0MTgwNgYDVQQDDC9BcHBsZSBEZXZlbG9wbWVudDogQ3JlYXRlZCB2aWEgQVBJIChWM0Q2TEdOMkY0KTETMBEGA1UECwwKODVITUEzWUhKWDEWMBQGA1UECgwNSm9uYXRoYW4gRGljazELMAkGA1UEBhMCVVMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCwsUPvpSh+xFnJ7UZTgok4vv6VgRqKXOeKiU5yRGfz3P+WYKX0hioGllU/RAMoEBVhqyRCXNvLnTAT0cyWrC8UP5P8EhSD9RT6NoVnAdV+hzSD5vAdKGIV0kdFoZiiWCFSBRLJf8gqNOrjCPEVjMeQLpGjILBW8/R/IHijS7W7Ab3rH4xZFlrT2lzbt51Yuy8jrPH/hB3w1Hgd0zaBop/q+v+UARGbaCPaHmWE/440qhFzyyEFbxrsIIXU5mEnIuWBsJFndF2JxhEMyjbwLZ+FIaGN/4yTkwwETG99OXRTU8fyUlSVYIXwKhwDkMd8HZ6ugOFSy0oQMkYuWQ5JRV0FAgMBAAGjggI4MIICNDAMBgNVHRMBAf8EAjAAMB8GA1UdIwQYMBaAFAn+wBWQ+a9kCpISuSYoYwyX7KeyMHAGCCsGAQUFBwEBBGQwYjAtBggrBgEFBQcwAoYhaHR0cDovL2NlcnRzLmFwcGxlLmNvbS93d2RyZzMuZGVyMDEGCCsGAQUFBzABhiVodHRwOi8vb2NzcC5hcHBsZS5jb20vb2NzcDAzLXd3ZHJnMzA0MIIBHgYDVR0gBIIBFTCCAREwggENBgkqhkiG92NkBQEwgf8wgcMGCCsGAQUFBwICMIG2DIGzUmVsaWFuY2Ugb24gdGhpcyBjZXJ0aWZpY2F0ZSBieSBhbnkgcGFydHkgYXNzdW1lcyBhY2NlcHRhbmNlIG9mIHRoZSB0aGVuIGFwcGxpY2FibGUgc3RhbmRhcmQgdGVybXMgYW5kIGNvbmRpdGlvbnMgb2YgdXNlLCBjZXJ0aWZpY2F0ZSBwb2xpY3kgYW5kIGNlcnRpZmljYXRpb24gcHJhY3RpY2Ugc3RhdGVtZW50cy4wNwYIKwYBBQUHAgEWK2h0dHBzOi8vd3d3LmFwcGxlLmNvbS9jZXJ0aWZpY2F0ZWF1dGhvcml0eS8wFgYDVR0lAQH/BAwwCgYIKwYBBQUHAwMwHQYDVR0OBBYEFOXBzAqsdMv1T4rb+r5Bl0awMnqKMA4GA1UdDwEB/wQEAwIHgDATBgoqhkiG92NkBgECAQH/BAIFADATBgoqhkiG92NkBgEMAQH/BAIFADANBgkqhkiG9w0BAQsFAAOCAQEAM1o1/WrgjFK+Us93U1wfxVloPkY8JimfWkNjohvv6eo+Tq6fiYWUGbgIhbM13Opy+pTFmpCf6HmW3r0G5FjAFo1CsWHtOn+nLZ7lUQG4FgPaT+eA3qFgPk/TBcQGO9dwRsgmDXkijcnlplxAKB4iDJca6Y8k5X2cNnTG0w9Rrhud5a2qDkvP8qK+erRzO85Wjq7qKSOOi1msfESU52TG95aLKHngLiIXhNw4/y9VKwAmlUPxf9xVmwlYID52qmKb5qqGWlEI5gqQjYCgWwzOmfsRaiFXi81tcWLm3nEcB+ZwfNrn7jb1w8tvMGz8fYxEOYkOssu+Joh9eQqqvZR83A==";

			var data = Convert.FromBase64String(content);

			var cert = new X509Certificate2(data);

			Assert.NotEmpty(cert.FriendlyName);
		}
	}
}
