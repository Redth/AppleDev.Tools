# JWT Authentication

## Overview

All App Store Connect API requests require a JWT (JSON Web Token) in the Authorization header:
```
Authorization: Bearer <token>
```

## Credentials Required

Obtain from App Store Connect → Users and Access → Integrations → App Store Connect API:

1. **Issuer ID** - Your team's issuer ID (UUID format)
2. **Key ID** - The ID of your API key (10 characters)
3. **Private Key (.p8)** - Download once; cannot re-download

## JWT Structure

### Header
```json
{
  "alg": "ES256",
  "kid": "YOUR_KEY_ID",
  "typ": "JWT"
}
```

### Payload
```json
{
  "iss": "YOUR_ISSUER_ID",
  "iat": 1706970000,
  "exp": 1706971200,
  "aud": "appstoreconnect-v1"
}
```

**Fields**:
- `iss` (issuer): Your Issuer ID
- `iat` (issued at): Current UNIX timestamp
- `exp` (expiration): UNIX timestamp, max 20 minutes from `iat`
- `aud` (audience): Always `"appstoreconnect-v1"`

## Token Lifetime

- **Maximum**: 20 minutes (1200 seconds)
- **Best practice**: Generate fresh token per request or cache briefly
- Expired tokens return HTTP 401

## Code Examples

### C# (.NET)
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

public string GenerateToken(string keyId, string issuerId, string privateKeyPem)
{
    var now = DateTimeOffset.UtcNow;
    var ecdsa = ECDsa.Create();
    ecdsa.ImportFromPem(privateKeyPem);
    
    var credentials = new SigningCredentials(
        new ECDsaSecurityKey(ecdsa) { KeyId = keyId },
        SecurityAlgorithms.EcdsaSha256);
    
    var token = new JwtSecurityToken(
        issuer: issuerId,
        audience: "appstoreconnect-v1",
        notBefore: now.DateTime,
        expires: now.AddMinutes(20).DateTime,
        signingCredentials: credentials);
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Python
```python
import jwt
import time

def generate_token(key_id: str, issuer_id: str, private_key: str) -> str:
    now = int(time.time())
    payload = {
        "iss": issuer_id,
        "iat": now,
        "exp": now + 1200,  # 20 minutes
        "aud": "appstoreconnect-v1"
    }
    return jwt.encode(
        payload,
        private_key,
        algorithm="ES256",
        headers={"kid": key_id}
    )
```

### Node.js
```javascript
const jwt = require('jsonwebtoken');

function generateToken(keyId, issuerId, privateKey) {
  const now = Math.floor(Date.now() / 1000);
  return jwt.sign(
    {
      iss: issuerId,
      iat: now,
      exp: now + 1200,
      aud: "appstoreconnect-v1"
    },
    privateKey,
    {
      algorithm: "ES256",
      keyid: keyId
    }
  );
}
```

### Go
```go
import (
    "crypto/ecdsa"
    "time"
    "github.com/golang-jwt/jwt/v5"
)

func GenerateToken(keyID, issuerID string, privateKey *ecdsa.PrivateKey) (string, error) {
    now := time.Now()
    claims := jwt.MapClaims{
        "iss": issuerID,
        "iat": now.Unix(),
        "exp": now.Add(20 * time.Minute).Unix(),
        "aud": "appstoreconnect-v1",
    }
    token := jwt.NewWithClaims(jwt.SigningMethodES256, claims)
    token.Header["kid"] = keyID
    return token.SignedString(privateKey)
}
```

## Private Key Format

The .p8 file contains an EC private key in PEM format:
```
-----BEGIN PRIVATE KEY-----
MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg...
-----END PRIVATE KEY-----
```

To load from base64-encoded environment variable:
```csharp
var privateKeyPem = Encoding.UTF8.GetString(Convert.FromBase64String(envVar));
```

## Scoped Tokens (Optional)

Limit token to specific operations:
```json
{
  "iss": "...",
  "iat": 1706970000,
  "exp": 1706971200,
  "aud": "appstoreconnect-v1",
  "scope": [
    "GET /v1/apps",
    "GET /v1/builds"
  ]
}
```

## Common Errors

| Error | Cause | Fix |
|-------|-------|-----|
| 401 Unauthorized | Token expired | Generate new token |
| 401 Unauthorized | Invalid signature | Check private key matches key ID |
| 403 Forbidden | Key lacks permission | Check API key roles in App Store Connect |
