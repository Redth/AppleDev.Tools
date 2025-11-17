# Apple Developer Tools MCP Server

This is a Model Context Protocol (MCP) server that exposes Apple development management functions, allowing AI assistants to help setup and manage Apple development environments.

## Features

The MCP server provides tools for managing:

### Device Management
- **ListDevices** - List registered Apple devices with optional filtering
- **RegisterDevice** - Register a new Apple device for development
- **ModifyDevice** - Modify an existing device's name or status

### Bundle ID Management
- **ListBundleIds** - List bundle IDs with optional filtering
- **CreateBundleId** - Create a new bundle ID
- **UpdateBundleId** - Update an existing bundle ID's name
- **DeleteBundleId** - Delete an existing bundle ID

### Certificate Management
- **ListCertificates** - List certificates with optional filtering
- **CreateCertificate** - Create a new certificate (auto-generates CSR)
- **RevokeCertificate** - Revoke (delete) a certificate

### Provisioning Profile Management
- **ListProvisioningProfiles** - List provisioning profiles with optional filtering
- **CreateProvisioningProfile** - Create a new provisioning profile
- **DeleteProvisioningProfile** - Delete a provisioning profile

## Setup

### Prerequisites

- .NET 9.0 SDK or later
- Apple Developer account with App Store Connect API access
- App Store Connect API credentials (Key ID, Issuer ID, Private Key)

### Configuration

The server requires three environment variables to be set:

```bash
export APP_STORE_CONNECT_KEY_ID="your-key-id"
export APP_STORE_CONNECT_ISSUER_ID="your-issuer-id"
export APP_STORE_CONNECT_PRIVATE_KEY="your-private-key-base64"
```

#### Multiple Credential Sets

The server supports multiple App Store Connect credential sets using a naming convention. This is useful when managing resources for multiple teams or organizations:

**Default credentials** (no suffix):
```bash
export APP_STORE_CONNECT_KEY_ID="default-key-id"
export APP_STORE_CONNECT_ISSUER_ID="default-issuer-id"
export APP_STORE_CONNECT_PRIVATE_KEY="default-private-key-base64"
```

**Named credential sets** (with suffix):
```bash
# ACME organization credentials
export APP_STORE_CONNECT_KEY_ID_ACME="acme-key-id"
export APP_STORE_CONNECT_ISSUER_ID_ACME="acme-issuer-id"
export APP_STORE_CONNECT_PRIVATE_KEY_ACME="acme-private-key-base64"

# Tailwind organization credentials
export APP_STORE_CONNECT_KEY_ID_TAILWIND="tailwind-key-id"
export APP_STORE_CONNECT_ISSUER_ID_TAILWIND="tailwind-issuer-id"
export APP_STORE_CONNECT_PRIVATE_KEY_TAILWIND="tailwind-private-key-base64"
```

All MCP tools accept an optional `keyName` parameter to specify which credential set to use. If not specified, the default credentials are used.

**Examples:**
- `ListDevices()` - Uses default credentials
- `ListDevices(keyName: "ACME")` - Uses ACME credentials
- `CreateBundleId(name: "MyApp", identifier: "com.example.app", platform: "IOS", keyName: "TAILWIND")` - Uses Tailwind credentials

### Building

```bash
dotnet build AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj
```

### Running

```bash
dotnet run --project AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj
```

## Architecture

The server is built using:

- **ModelContextProtocol SDK** (v0.4.0-preview.3) - Official C# SDK for MCP servers
- **AppStoreConnectClient** - Existing library that provides comprehensive App Store Connect API integration
- **.NET 9.0** - Latest .NET runtime

The implementation uses the MCP SDK's `[McpServerToolType]` attribute on a static class containing tool methods marked with `[McpServerTool]` attributes. Tools are automatically discovered via `WithToolsFromAssembly()`.

## Integration with Claude Code

Once running, this MCP server can be configured in Claude Code to provide Apple development management capabilities. The AI assistant can then help with tasks like:

- Setting up devices for development
- Creating and managing bundle IDs
- Managing certificates and provisioning profiles
- Troubleshooting development environment issues

## Status

**Current Status**: In development

### Completed
- Project structure and dependencies
- MCP server setup with Host builder pattern
- All 15 tool method implementations covering devices, bundle IDs, certificates, and provisioning profiles
- Integration with existing AppStoreConnectClient library
- Multi-credential set support with environment variable naming convention
- Build succeeds with 0 errors

### Remaining
- Testing with actual App Store Connect API credentials
- Documentation for MCP client configuration
- Additional tools for simulators, physical devices, keychain, Xcode, and CI operations (future enhancement)

## License

Same as parent AppleDev.Tools project.
