---
name: app-store-connect-api
description: >
  Guide for implementing Apple App Store Connect API integrations. Use when:
  (1) Building API clients to manage certificates, provisioning profiles, bundle IDs, or devices,
  (2) Implementing JWT authentication for App Store Connect,
  (3) Understanding API request/response patterns and error handling,
  (4) Working with TestFlight builds, beta groups, or app submissions,
  (5) Discovering available endpoints, schemas, or capabilities via the OpenAPI spec.
  Includes scripts for fetching and analyzing Apple's official OpenAPI specification.
---

# App Store Connect API Development

## Quick Reference

**Base URL**: `https://api.appstoreconnect.apple.com/`  
**Auth**: JWT with ES256 signature (max 20 min lifetime)  
**Format**: JSON:API specification

## Authentication

Generate JWT with these claims:
```json
{
  "iss": "ISSUER_ID",      // From App Store Connect
  "iat": <unix_timestamp>,
  "exp": <iat + 1200>,     // Max 20 minutes
  "aud": "appstoreconnect-v1"
}
```

Header: `{ "alg": "ES256", "kid": "KEY_ID", "typ": "JWT" }`

For code examples in C#, Python, Node.js, Go → see [references/authentication.md](references/authentication.md)

## Request/Response Format

All resources follow JSON:API structure:

```json
// Response
{
  "data": { "type": "bundleIds", "id": "ABC123", "attributes": {...} },
  "included": [...],  // Related resources when using ?include=
  "links": { "self": "...", "next": "..." }
}

// Create/Update Request
{
  "data": {
    "type": "bundleIds",
    "attributes": { "name": "My App", "identifier": "com.example.app", "platform": "IOS" },
    "relationships": { ... }
  }
}
```

Common query parameters:
- `filter[field]=value` - Filter results
- `sort=field` / `sort=-field` - Sort ascending/descending  
- `limit=50` - Pagination (follow `links.next`)
- `include=relatedResource` - Include related data
- `fields[type]=field1,field2` - Sparse fieldsets

For full patterns → see [references/patterns.md](references/patterns.md)

## Core Resources

| Resource | Endpoint | Key Operations |
|----------|----------|----------------|
| Certificates | `/v1/certificates` | List, Create (CSR), Delete |
| Bundle IDs | `/v1/bundleIds` | List, Create, Update name, Delete |
| Capabilities | `/v1/bundleIdCapabilities` | Enable, Update, Disable |
| Devices | `/v1/devices` | List, Register, Update status |
| Profiles | `/v1/profiles` | List, Create, Delete |
| Apps | `/v1/apps` | List, Get, Update |
| Builds | `/v1/builds` | List, Get, Update (expire) |

For detailed schemas and request formats → see [references/resources.md](references/resources.md)

## Common Enums

**Platform**: `IOS`, `MAC_OS`, `UNIVERSAL`

**CertificateType**: `IOS_DEVELOPMENT`, `IOS_DISTRIBUTION`, `MAC_APP_DEVELOPMENT`, `MAC_APP_DISTRIBUTION`, `DEVELOPER_ID_APPLICATION`

**ProfileType**: `IOS_APP_DEVELOPMENT`, `IOS_APP_STORE`, `IOS_APP_ADHOC`, `MAC_APP_DEVELOPMENT`, `MAC_APP_STORE`

**CapabilityType**: `PUSH_NOTIFICATIONS`, `ICLOUD`, `GAME_CENTER`, `IN_APP_PURCHASE`, `APP_GROUPS`, `APPLE_PAY`, `ASSOCIATED_DOMAINS`, `HEALTHKIT`, `HOMEKIT`, `SIRIKIT`

## Discovering Endpoints & Schemas

Use the OpenAPI spec analyzer script to explore the full API:

```powershell
# Download spec and show summary
./scripts/fetch_openapi_spec.ps1

# List all 192 resource categories
./scripts/fetch_openapi_spec.ps1 -ListResources

# Show endpoints for a resource
./scripts/fetch_openapi_spec.ps1 -Resource BundleIdCapabilities

# Show schema details
./scripts/fetch_openapi_spec.ps1 -Schema BundleIdCreateRequest

# Search endpoints and schemas
./scripts/fetch_openapi_spec.ps1 -Search testflight
```

The script caches the spec at `~/.cache/asc-openapi/openapi.oas.json`. Use `-Refresh` to re-download.

## Error Handling

All errors return:
```json
{
  "errors": [{
    "status": "400",
    "code": "PARAMETER_ERROR.INVALID",
    "title": "...",
    "detail": "..."
  }]
}
```

Common codes:
- `PARAMETER_ERROR.INVALID` / `PARAMETER_ERROR.MISSING` - Bad request
- `NOT_FOUND` - Resource doesn't exist
- `FORBIDDEN` - Insufficient API key permissions
- `AUTHENTICATION_ERROR` - JWT issues (expired, bad signature)
- `429` - Rate limited (check `Retry-After` header)

## Implementation Tips

1. **Token caching**: Generate one token and reuse for ~15 min (leave buffer before 20 min expiry)

2. **Pagination**: Always follow `links.next` until exhausted; don't assume result counts

3. **Relationship endpoints**: Use `/v1/bundleIds/{id}/bundleIdCapabilities` not filter params (some relationships don't support filtering)

4. **Wildcard bundle IDs**: End with `.*` (e.g., `com.example.*`); have limited capability support

5. **Profile devices**: Development profiles require explicit device list; App Store profiles don't include devices

6. **Certificate CSR**: Generate locally with `openssl` or programmatically; submit base64-encoded content

## Official Resources

- [API Documentation](https://developer.apple.com/documentation/appstoreconnectapi)
- [OpenAPI Spec Download](https://developer.apple.com/sample-code/app-store-connect/app-store-connect-openapi-specification.zip)
- [Creating API Keys](https://developer.apple.com/documentation/appstoreconnectapi/creating_api_keys_for_app_store_connect_api)
- [Generating Tokens](https://developer.apple.com/documentation/appstoreconnectapi/generating_tokens_for_api_requests)
