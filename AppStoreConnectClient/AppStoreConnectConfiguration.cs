using JWT.Algorithms;
using JWT.Builder;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;

namespace AppleAppStoreConnect;

public class AppStoreConnectConfiguration
{
	public AppStoreConnectConfiguration(string keyId, string issuerId, string privateKeyBase64)
	{
		PrivateKeyRaw = privateKeyBase64 ?? string.Empty;
		PrivateKeyBase64 = NormalizeToBase64Body(PrivateKeyRaw);
		KeyId = keyId;
		IssuerId = issuerId;
	}

	// Raw input (may still contain headers or escaped newlines)
	public string PrivateKeyRaw { get; }

	string accessToken = string.Empty;
	public string AccessToken
	{
		get
		{
			// Refresh if missing or expiring within 1 minute
			if (string.IsNullOrEmpty(currentJwt) || currentJwtExpires <= DateTime.UtcNow.AddMinutes(1))
			{
				accessToken = GetJwt();
			}
			return accessToken;
		}
		set => accessToken = value;
	}

	public string PrivateKeyBase64 { get; set; }
	public string KeyId { get; set; }
	public string IssuerId { get; set; }

	string currentJwt = string.Empty;
	DateTime currentJwtExpires = DateTime.UtcNow;
	readonly object jwtLock = new();

	string GetJwt()
	{
		lock (jwtLock)
		{
			if (!string.IsNullOrEmpty(currentJwt) && currentJwtExpires > DateTime.UtcNow.AddMinutes(1))
				return currentJwt;

			var pk = CreateEcdsaFromKeyMaterial(PrivateKeyRaw, PrivateKeyBase64);

			var now = DateTimeOffset.UtcNow;
			var exp = now.AddMinutes(19); // Apple requires <=20 minutes

			var jwtBuilder = new JwtBuilder()
				.WithAlgorithm(new ES256Algorithm(ECDsa.Create(), pk))
				.AddHeader("kid", KeyId)
				.AddClaim("iss", IssuerId)
				.AddClaim("iat", now.ToUnixTimeSeconds())
				.AddClaim("exp", exp.ToUnixTimeSeconds())
				.AddClaim("aud", "appstoreconnect-v1");

			currentJwt = jwtBuilder.Encode();
			currentJwtExpires = exp.UtcDateTime;
			return currentJwt;
		}
	}

	// Regex for generic PKCS#8 PRIVATE KEY block
	static Regex rxPrvKey = new(@"(-){1,}BEGIN\s+PRIVATE\s+KEY(-){1,}\s*(?<pk>[A-Za-z0-9+/=\r\n]+?)\s*(-){1,}END\s+PRIVATE\s+KEY(-){1,}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
	// Regex for EC PRIVATE KEY (PKCS#1 style)
	static Regex rxEcPrvKey = new(@"(-){1,}BEGIN\s+EC\s+PRIVATE\s+KEY(-){1,}\s*(?<pk>[A-Za-z0-9+/=\r\n]+?)\s*(-){1,}END\s+EC\s+PRIVATE\s+KEY(-){1,}", RegexOptions.Singleline | RegexOptions.IgnoreCase);

	static string NormalizeToBase64Body(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return string.Empty;

		var candidate = input.Trim();
		candidate = candidate.Contains("\\n") && !candidate.Contains("\n") ? candidate.Replace("\\n", "\n") : candidate;

		bool HasPemHeaders(string txt) => txt.Contains("BEGIN PRIVATE KEY") || txt.Contains("BEGIN EC PRIVATE KEY");

		// 1. Direct PEM
		if (HasPemHeaders(candidate))
		{
			var body = ExtractPrivateKeyChars(candidate);
			if (IsBase64(body)) return body;
		}

		// 2. Base64 of PEM or body
		if (IsBase64(candidate))
		{
			try
			{
				var firstDecode = Convert.FromBase64String(candidate);
				var asText = Encoding.UTF8.GetString(firstDecode);

				// 2a. Decoded text contains PEM headers
				if (HasPemHeaders(asText))
				{
					var body = ExtractPrivateKeyChars(asText);
					if (IsBase64(body)) return body;
				}
				// 2b. Decoded text itself looks like base64 body and decodes to ASN.1 (starts with 0x30)
				if (IsBase64(asText))
				{
					var inner = Convert.FromBase64String(asText);
					if (inner.Length > 0 && inner[0] == 0x30) // ASN.1 SEQUENCE tag
						return asText;
				}
				// 2c. If firstDecode itself starts with ASN.1 sequence (0x30) then original candidate was DER base64 body
				if (firstDecode.Length > 0 && firstDecode[0] == 0x30)
					return candidate; // already body
			}
			catch { /* ignore */ }
		}

		// Fallback: assume already base64 body
		return candidate;
	}

	static bool IsBase64(string s)
	{
		if (string.IsNullOrWhiteSpace(s)) return false;
		var trimmed = s.Trim();
		if (trimmed.Length % 4 != 0) return false;
		// Quick char check
		for (int i = 0; i < trimmed.Length; i++)
		{
			char c = trimmed[i];
			bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '=';
			if (!ok) return false;
		}
		try { Convert.FromBase64String(trimmed); return true; } catch { return false; }
	}

	// Regex for generic PKCS#8 PRIVATE KEY block
	static Regex rxPrvKey2 = new(@"(-){1,}BEGIN\s+PRIVATE\s+KEY(-){1,}\s*(?<pk>[A-Za-z0-9+/=\r\n]+?)\s*(-){1,}END\s+PRIVATE\s+KEY(-){1,}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
	// Regex for EC PRIVATE KEY (PKCS#1 style)
	static Regex rxEcPrvKey2 = new(@"(-){1,}BEGIN\s+EC\s+PRIVATE\s+KEY(-){1,}\s*(?<pk>[A-Za-z0-9+/=\r\n]+?)\s*(-){1,}END\s+EC\s+PRIVATE\s+KEY(-){1,}", RegexOptions.Singleline | RegexOptions.IgnoreCase);

	static string ExtractPrivateKeyChars(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return string.Empty;

		// If the content uses escaped \n sequences (env var style), convert them to real newlines first
		text = text.Contains("\\n") && !text.Contains("\n") ? text.Replace("\\n", "\n") : text;

		var match = rxPrvKey2.Match(text);
		if (match.Success && match.Groups.TryGetValue("pk", out var pk1))
			return StripWhitespace(pk1.Value);

		match = rxEcPrvKey2.Match(text);
		if (match.Success && match.Groups.TryGetValue("pk", out var pk2))
			return StripWhitespace(pk2.Value);

		// If no headers, assume it's already base64 body; return trimmed
		return StripWhitespace(text);
	}

	static string StripWhitespace(string s)
		=> string.Join(string.Empty, s.Split(new[] { '\r', '\n', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));

	static ECDsa CreateEcdsaFromKeyMaterial(string originalInput, string base64Body)
	{
		var pkcs8Bytes = SafeBase64Decode(base64Body);
		var ecdsa = ECDsa.Create();
		try
		{
			ecdsa.ImportPkcs8PrivateKey(pkcs8Bytes, out _);
			return ecdsa;
		}
		catch (Exception ex)
		{
			// Attempt EC PRIVATE KEY fallback if original contained that header
			if (rxEcPrvKey.IsMatch(originalInput))
			{
				try
				{
					var ecBody = rxEcPrvKey.Match(originalInput).Groups["pk"].Value;
					var ecBytes = SafeBase64Decode(ecBody);
					var ec = ECDsa.Create();
					ec.ImportECPrivateKey(ecBytes, out _);
					return ec;
				}
				catch (Exception ex2)
				{
					throw new CryptographicException("Failed to parse EC PRIVATE KEY block for App Store Connect API key.", ex2);
				}
			}

			throw new CryptographicException("Failed to parse PKCS#8 PRIVATE KEY for App Store Connect API key. Ensure you are using the .p8 file contents (BEGIN PRIVATE KEY).", ex);
		}
	}

	static byte[] SafeBase64Decode(string base64)
	{
		try
		{
			return Convert.FromBase64String(base64);
		}
		catch (FormatException fe)
		{
			throw new CryptographicException("Private key is not valid Base64. If passing via environment variable, ensure it is the raw .p8 contents.", fe);
		}
	}
}