using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppleAppStoreConnect;

partial class AppStoreConnectClient
{
	public const string CERTIFICATES_TYPE = Certificate.TYPE;

	public async Task<CertificateResponse> ListCertificatesAsync(
		string[]? filterId = null,
		string[]? filterDisplayName = null,
		string[]? filterSerialNumber = null,
		CertificateType[]? filterCertificateType = null,
		string? sort = null,
		int? limit = null,
		string[]? fieldsCertificates = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();

		qs.Filter("id", filterId);
		qs.Filter("displayName", filterDisplayName);
		qs.Filter("serialNumber", filterSerialNumber);
		qs.Filter("certificateType", filterCertificateType);

		qs.Sort(sort, "id", "-id", "displayName", "-displayName", "serialNumber", "-serialNumber", "certificateType", "-certificateType");

		qs.Limit(nameof(limit), null, 200, limit);

		qs.Fields(CERTIFICATES_TYPE, fieldsCertificates);
		
		return await RequestAsync<CertificateResponse>(CERTIFICATES_TYPE, qs, cancellationToken).ConfigureAwait(false)
			?? new CertificateResponse();
	}

	public async Task<ItemResponse<Certificate, CertificateAttributes>> CreateCertificateAsync(
		string csrContent,
		CertificateType certificateType,
		CancellationToken cancellationToken = default)
	{
		var request = new CreateCertificateRequestAttributes(csrContent,
				certificateType);

		return await PostAsync<Certificate, CertificateAttributes, CreateCertificateRequestAttributes>(
			CERTIFICATES_TYPE,
			request,
			cancellationToken)
			.ConfigureAwait(false)
				?? new ItemResponse<Certificate, CertificateAttributes>();
	}


	public static string GenerateCertificateSigningRequestPem(string? commonName = null)
	{
		using var rsa = RSA.Create(2048);

		var certificateRequest = new CertificateRequest(
			$"CN={commonName ?? Environment.MachineName}",
			rsa,
			HashAlgorithmName.SHA256,
			RSASignaturePadding.Pkcs1);

		return certificateRequest.CreateSigningRequestPem();
	}

	public Task<bool> RevokeCertificateAsync(string id, CancellationToken cancellationToken = default)
		=> DeleteAsync($"{CERTIFICATES_TYPE}/{id}", cancellationToken);
}
