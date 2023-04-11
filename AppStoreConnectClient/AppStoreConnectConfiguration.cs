using JWT.Algorithms;
using JWT.Builder;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AppleAppStoreConnect;

public class AppStoreConnectConfiguration
{
	public AppStoreConnectConfiguration(string keyId, string issuerId, string privateKeyBase64)
	{
		PrivateKeyBase64 = ExtractPrivateKeyChars(privateKeyBase64);
		KeyId = keyId;
		IssuerId = issuerId;
	}

	string accessToken = string.Empty;
	public string AccessToken
	{
		get
		{
			if (string.IsNullOrEmpty(accessToken) || currentJwtExpires < DateTime.UtcNow.AddMinutes(19))
				accessToken = GetJwt();

			return accessToken;
		}
		set
		{
			accessToken = value;
		}
	}


	public string PrivateKeyBase64 { get; set; }
	public string KeyId { get; set; }
	public string IssuerId { get; set; }

	string currentJwt = string.Empty;
	DateTime currentJwtExpires = DateTime.UtcNow;

	string GetJwt()
	{
		if (!string.IsNullOrEmpty(currentJwt) && currentJwtExpires > DateTime.UtcNow.AddMinutes(1))
		{
			return currentJwt;
		}

		var pk = ECDsa.Create();
		pk.ImportPkcs8PrivateKey(Convert.FromBase64String(PrivateKeyBase64), out var bytesRead);

		var now = DateTimeOffset.UtcNow;
		var exp = now.AddMinutes(19);

		var jwtBuilder = new JwtBuilder()
			.WithAlgorithm(new ES256Algorithm(ECDsa.Create(), pk))
			.AddHeader("kid", KeyId);

		var payload = new Dictionary<string, object>
		{
			{ "iss", IssuerId },
			{ "iat", now.ToUnixTimeSeconds() },
			{ "exp", exp.ToUnixTimeSeconds() },
			{ "aud", "appstoreconnect-v1" }
		};
		return jwtBuilder.Encode(payload);
	}

	static Regex rxPrvKey = new Regex(@"(-){1,}.*?BEGIN.*?PRIVATE.*?(-){1,}\s{0,}(?<pk>.*?)\s{0,}(-){1,}.*?END.*?PRIVATE.*?(-){1,}", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

	static string ExtractPrivateKeyChars(string text)
	{
		var match = rxPrvKey.Match(text);

		if (match?.Success ?? false)
		{
			if (match.Groups.TryGetValue("pk", out var pk))
			{
				return pk.Value;
			}
		}

		return text;
	}
}