# Core Resources Reference

## Certificates

**Endpoints**: `/v1/certificates`

**Operations**:
- List: `GET /v1/certificates`
- Get: `GET /v1/certificates/{id}`
- Create: `POST /v1/certificates`
- Delete: `DELETE /v1/certificates/{id}`

**CertificateType enum**:
- `IOS_DEVELOPMENT`, `IOS_DISTRIBUTION`
- `MAC_APP_DEVELOPMENT`, `MAC_APP_DISTRIBUTION`, `MAC_INSTALLER_DISTRIBUTION`
- `DEVELOPER_ID_KEXT`, `DEVELOPER_ID_APPLICATION`, `DEVELOPER_ID_INSTALLER`

**Create Request**:
```json
{
  "data": {
    "type": "certificates",
    "attributes": {
      "certificateType": "IOS_DISTRIBUTION",
      "csrContent": "-----BEGIN CERTIFICATE REQUEST-----..."
    }
  }
}
```

## Bundle IDs

**Endpoints**: `/v1/bundleIds`

**Operations**:
- List: `GET /v1/bundleIds`
- Get: `GET /v1/bundleIds/{id}`
- Create: `POST /v1/bundleIds`
- Update: `PATCH /v1/bundleIds/{id}` (name only)
- Delete: `DELETE /v1/bundleIds/{id}`
- Capabilities: `GET /v1/bundleIds/{id}/bundleIdCapabilities`

**Platform enum**: `IOS`, `MAC_OS`, `UNIVERSAL`

**Create Request**:
```json
{
  "data": {
    "type": "bundleIds",
    "attributes": {
      "identifier": "com.example.myapp",
      "name": "My App",
      "platform": "IOS",
      "seedId": "TEAMID123"  // Optional
    }
  }
}
```

**Wildcard vs Explicit**:
- Explicit: `com.example.myapp` (specific app)
- Wildcard: `com.example.*` (multiple apps, limited capabilities)

## Bundle ID Capabilities

**Endpoints**: `/v1/bundleIdCapabilities`

**Operations**:
- Enable: `POST /v1/bundleIdCapabilities`
- Update: `PATCH /v1/bundleIdCapabilities/{id}`
- Disable: `DELETE /v1/bundleIdCapabilities/{id}`

**CapabilityType enum** (common):
- `ICLOUD`, `PUSH_NOTIFICATIONS`, `GAME_CENTER`, `IN_APP_PURCHASE`
- `APP_GROUPS`, `APPLE_PAY`, `ASSOCIATED_DOMAINS`, `HEALTHKIT`
- `HOMEKIT`, `SIRIKIT`, `WALLET`, `NFC_TAG_READING`, `APPLE_ID_AUTH`
- `DATA_PROTECTION`, `MAPS`, `NETWORK_EXTENSIONS`

**Enable Request**:
```json
{
  "data": {
    "type": "bundleIdCapabilities",
    "attributes": {
      "capabilityType": "PUSH_NOTIFICATIONS"
    },
    "relationships": {
      "bundleId": {
        "data": { "type": "bundleIds", "id": "BUNDLE_ID_RESOURCE_ID" }
      }
    }
  }
}
```

## Devices

**Endpoints**: `/v1/devices`

**Operations**:
- List: `GET /v1/devices`
- Get: `GET /v1/devices/{id}`
- Register: `POST /v1/devices`
- Update: `PATCH /v1/devices/{id}`

**Platform enum**: `IOS`, `MAC_OS`  
**DeviceStatus enum**: `ENABLED`, `DISABLED`

**Register Request**:
```json
{
  "data": {
    "type": "devices",
    "attributes": {
      "name": "John's iPhone",
      "platform": "IOS",
      "udid": "00008030-001234567890001E"
    }
  }
}
```

## Profiles (Provisioning)

**Endpoints**: `/v1/profiles`

**Operations**:
- List: `GET /v1/profiles`
- Get: `GET /v1/profiles/{id}`
- Create: `POST /v1/profiles`
- Delete: `DELETE /v1/profiles/{id}`

**ProfileType enum**:
- Development: `IOS_APP_DEVELOPMENT`, `MAC_APP_DEVELOPMENT`
- Distribution: `IOS_APP_STORE`, `IOS_APP_ADHOC`, `IOS_APP_INHOUSE`
- Mac: `MAC_APP_STORE`, `MAC_APP_DIRECT`, `MAC_CATALYST_APP_STORE`

**ProfileState enum**: `ACTIVE`, `INVALID`

**Create Request**:
```json
{
  "data": {
    "type": "profiles",
    "attributes": {
      "name": "My App Development",
      "profileType": "IOS_APP_DEVELOPMENT"
    },
    "relationships": {
      "bundleId": {
        "data": { "type": "bundleIds", "id": "BUNDLE_ID" }
      },
      "certificates": {
        "data": [{ "type": "certificates", "id": "CERT_ID" }]
      },
      "devices": {
        "data": [
          { "type": "devices", "id": "DEVICE_ID_1" },
          { "type": "devices", "id": "DEVICE_ID_2" }
        ]
      }
    }
  }
}
```

## Apps

**Endpoints**: `/v1/apps`

**Operations**:
- List: `GET /v1/apps`
- Get: `GET /v1/apps/{id}`
- Update: `PATCH /v1/apps/{id}`

Apps are read-mostly; created via App Store Connect UI or first build upload.

**Related Resources**:
- App Store Versions: `GET /v1/apps/{id}/appStoreVersions`
- Pre-release Versions: `GET /v1/apps/{id}/preReleaseVersions`
- Builds: `GET /v1/apps/{id}/builds`
- Beta Groups: `GET /v1/apps/{id}/betaGroups`

## Users

**Endpoints**: `/v1/users`

**Operations**:
- List: `GET /v1/users`
- Get: `GET /v1/users/{id}`
- Update: `PATCH /v1/users/{id}`
- Delete: `DELETE /v1/users/{id}`
- Invite: `POST /v1/userInvitations`

**UserRole enum** (subset):
- `ADMIN`, `FINANCE`, `ACCESS_TO_REPORTS`, `SALES`
- `DEVELOPER`, `APP_MANAGER`, `CUSTOMER_SUPPORT`, `MARKETING`

## Builds & TestFlight

**Build Endpoints**: `/v1/builds`
- List builds: `GET /v1/builds`
- Get build: `GET /v1/builds/{id}`
- Update build (expire): `PATCH /v1/builds/{id}`

**Beta Groups**: `/v1/betaGroups`
- Create/manage groups of TestFlight testers

**Beta Testers**: `/v1/betaTesters`
- Add/remove testers from beta groups
