using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppleAppStoreConnect;

public class CertificateSigningRequestGenerator
{
	public string GeneratePem(string? commonName = null)
	{
		using var rsa = RSA.Create(2048);

		var certificateRequest = new CertificateRequest(
			$"CN={commonName ?? Environment.MachineName}",
			rsa,
			HashAlgorithmName.SHA256,
			RSASignaturePadding.Pkcs1);

		return certificateRequest.CreateSigningRequestPem();
	}
}