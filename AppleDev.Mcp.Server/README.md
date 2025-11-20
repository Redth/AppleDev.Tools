# Apple Developer Tools MCP Server

A comprehensive [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server providing **44 tools** for Apple development automation. Enable AI assistants to manage iOS/macOS/tvOS/watchOS/visionOS development environments, certificates, provisioning profiles, simulators, and more.

## 🚀 Quick Start

### Installation

The server is distributed as a .NET tool and can be installed/run in several ways:

#### Option 1: Install as Global Tool (Recommended)

```bash
dotnet tool install -g AppleDev.Mcp.Server
```

Then use the `appledev-mcp` command directly in your MCP configuration.

#### Option 2: Run with `dotnet tool run` (No Installation)

```bash
dotnet tool run --global appledev-mcp
```

#### Option 3: Run with `dnx` (Download and Run in One Shot)

```bash
dotnet dnx -y AppleDev.Mcp.Server
```

> NOTE: Requires .NET 10 SDK for the dotnet dnx command

This downloads and runs the tool without installing it globally.

### MCP Server Configuration

Add this configuration to your MCP client settings (e.g., Claude Desktop, Cline, etc.):

#### Using Installed Tool (Recommended)

After installing with `dotnet tool install -g AppleDev.Mcp.Server`:

```json
{
  "mcpServers": {
    "apple-dev": {
      "command": "appledev-mcp",
      "env": {
        "APP_STORE_CONNECT_KEY_ID": "your-key-id",
        "APP_STORE_CONNECT_ISSUER_ID": "your-issuer-id",
        "APP_STORE_CONNECT_PRIVATE_KEY": "base64-encoded-private-key"
      }
    }
  }
}
```

#### Using `dnx` (No Installation Required)

```json
{
  "mcpServers": {
    "apple-dev": {
      "command": "dotnet",
      "args": ["dnx", "-y", "AppleDev.Mcp.Server"],
      "env": {
        "APP_STORE_CONNECT_KEY_ID": "your-key-id",
        "APP_STORE_CONNECT_ISSUER_ID": "your-issuer-id",
        "APP_STORE_CONNECT_PRIVATE_KEY": "base64-encoded-private-key"
      }
    }
  }
}
```

#### Using Source (Development)

```json
{
  "mcpServers": {
    "apple-dev": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/AppleDev.Tools/AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj"
      ],
      "env": {
        "APP_STORE_CONNECT_KEY_ID": "your-key-id",
        "APP_STORE_CONNECT_ISSUER_ID": "your-issuer-id",
        "APP_STORE_CONNECT_PRIVATE_KEY": "base64-encoded-private-key"
      }
    }
  }
}
```

#### Claude Desktop Configuration

For Claude Desktop specifically, edit the configuration file:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

**Using Installed Tool (Simplest):**
```json
{
  "mcpServers": {
    "apple-dev": {
      "command": "appledev-mcp",
      "env": {
        "APP_STORE_CONNECT_KEY_ID": "ABCD1234",
        "APP_STORE_CONNECT_ISSUER_ID": "12345678-1234-1234-1234-123456789012",
        "APP_STORE_CONNECT_PRIVATE_KEY": "LS0tLS1CRUdJTi..."
      }
    }
  }
}
```

**Using dnx (No Installation):**
```json
{
  "mcpServers": {
    "apple-dev": {
      "command": "dnx",
      "args": ["run", "appledev-mcp"],
      "env": {
        "APP_STORE_CONNECT_KEY_ID": "ABCD1234",
        "APP_STORE_CONNECT_ISSUER_ID": "12345678-1234-1234-1234-123456789012",
        "APP_STORE_CONNECT_PRIVATE_KEY": "LS0tLS1CRUdJTi..."
      }
    }
  }
}
```

### Multiple Credential Sets

Support multiple Apple Developer accounts:

```json
{
  "mcpServers": {
    "apple-dev": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj"],
      "env": {
        "APP_STORE_CONNECT_KEY_ID": "default-key",
        "APP_STORE_CONNECT_ISSUER_ID": "default-issuer",
        "APP_STORE_CONNECT_PRIVATE_KEY": "default-key-base64",
        "APP_STORE_CONNECT_KEY_ID_COMPANY_A": "company-a-key",
        "APP_STORE_CONNECT_ISSUER_ID_COMPANY_A": "company-a-issuer",
        "APP_STORE_CONNECT_PRIVATE_KEY_COMPANY_A": "company-a-key-base64"
      }
    }
  }
}
```

Then use tools with the `keyName` parameter:
- `ListDevices()` - uses default credentials
- `ListDevices(keyName: "COMPANY_A")` - uses Company A credentials

## 📋 Prerequisites

- **.NET 9.0 SDK** or later
- **macOS** (for keychain, simulator, and device tools)
- **Xcode** (for simulator and device tools)
- **App Store Connect API** credentials:
  - Key ID
  - Issuer ID (Team ID)
  - Private Key (.p8 file)

### Getting App Store Connect API Credentials

1. Go to [App Store Connect](https://appstoreconnect.apple.com)
2. Navigate to **Users and Access** → **Keys**
3. Create a new API key with appropriate access
4. Download the `.p8` private key file
5. Note the **Key ID** and **Issuer ID**
6. Convert private key to base64:
   ```bash
   base64 -i AuthKey_ABCD1234.p8 | tr -d '\n'
   ```

## 🛠️ Available Tools (44 Total)

### App Store Connect - Devices (3 tools)

Manage registered development and testing devices.

- **`ListDevices`** - List registered devices with filtering
  - Filters: platform, status, name, UDID
  - Returns: Device name, UDID, platform, model, status

- **`RegisterDevice`** - Register a new device
  - Input: name, UDID, platform (IOS, MAC_OS, TVOS, VISIONOS)
  - Returns: Registration confirmation with device details

- **`ModifyDevice`** - Update device name or status
  - Input: deviceId, new name, enabled status
  - Returns: Update confirmation

### App Store Connect - Bundle IDs (4 tools)

Manage app bundle identifiers.

- **`ListBundleIds`** - List bundle IDs with filtering
  - Filters: identifier, name, platform
  - Returns: Bundle ID details, identifier, name, platform

- **`CreateBundleId`** - Create new bundle ID
  - Input: name, identifier (e.g., com.company.app), platform
  - Returns: Created bundle ID details

- **`UpdateBundleId`** - Update bundle ID name
  - Input: bundleId, new name
  - Returns: Update confirmation

- **`DeleteBundleId`** - Delete a bundle ID
  - Input: bundleId
  - Returns: Deletion confirmation

### App Store Connect - Certificates (3 tools)

Manage signing certificates.

- **`ListCertificates`** - List certificates with filtering
  - Filters: certificate type, display name
  - Returns: Certificate details, type, expiration, serial number

- **`CreateCertificate`** - Create new certificate (auto-generates CSR)
  - Input: certificate type, common name, email
  - Returns: Certificate details and content

- **`RevokeCertificate`** - Revoke (delete) a certificate
  - Input: certificateId
  - Returns: Revocation confirmation

### App Store Connect - Provisioning Profiles (7 tools)

Manage provisioning profiles.

- **`ListProvisioningProfiles`** - List profiles with filtering
  - Filters: profile type, state, name
  - Returns: Profile details, UUID, type, state, expiration

- **`CreateProvisioningProfile`** - Create new profile
  - Input: name, profile type, bundle ID, certificate IDs, device IDs
  - Returns: Created profile details

- **`DeleteProvisioningProfile`** - Delete a profile
  - Input: profileId
  - Returns: Deletion confirmation

- **`ListInstalledProvisioningProfiles`** - List locally installed profiles
  - Returns: Installed profiles with UUID, name, team, expiration

- **`ParseProvisioningProfile`** - Parse and display profile contents
  - Input: file path to .mobileprovision or .provisionprofile
  - Returns: Complete profile details, certificates, devices, entitlements

- **`DownloadProvisioningProfile`** - Download profile from App Store Connect
  - Input: profileId
  - Returns: Profile metadata and base64-encoded content

- **`InstallProvisioningProfile`** - Install profile locally
  - Input: base64Data OR filePath
  - Returns: Installation status, UUID, installation path

> **Note:** To add devices to an existing profile, use the workflow: List devices → Delete old profile → Create new profile with updated device list → Download → Install

### Physical Devices (1 tool)

List connected physical devices and simulators.

- **`ListDevicesAndSimulators`** - List devices via xcdevice
  - Input: timeout (seconds), devicesOnly flag
  - Returns: Connected devices with UDID, name, OS version, model, interface

### Xcode (2 tools)

Manage Xcode installations.

- **`ListXcode`** - List all installed Xcode versions
  - Returns: Xcode paths, versions, selected status

- **`LocateXcode`** - Find active or best Xcode installation
  - Input: best flag (find highest version vs. currently selected)
  - Returns: Xcode path and version

### App Operations (1 tool)

Extract information from app bundles.

- **`GetAppInfo`** - Get app bundle information
  - Input: path to .app bundle
  - Returns: Bundle ID, version, build number, platform, SDK info

### Simulators (15 tools)

Complete iOS/tvOS/watchOS/visionOS simulator management.

**Simulator Lifecycle:**
- **`ListSimulators`** - List simulators with extensive filtering
  - Filters: booted, available, unavailable, name, UDID, runtime, device type
  - Returns: Simulator details, state, runtime, device type

- **`CreateSimulator`** - Create new simulator
  - Input: name, device type, runtime (optional)
  - Returns: Creation confirmation

- **`DeleteSimulator`** - Delete simulator(s)
  - Input: target (UDID, name, 'all', or status)
  - Returns: Deletion confirmation

- **`BootSimulator`** - Boot a simulator
  - Input: target (UDID or name)
  - Returns: Boot status

- **`ShutdownSimulator`** - Shutdown simulator(s)
  - Input: target (default: 'all')
  - Returns: Shutdown confirmation

- **`EraseSimulator`** - Erase all content from simulator
  - Input: target (UDID or name)
  - Returns: Erase confirmation

- **`OpenSimulator`** - Open Simulator.app
  - Input: UDID (optional - opens specific simulator)
  - Returns: Open status

**Simulator Discovery:**
- **`ListSimulatorDeviceTypes`** - List available device types
  - Returns: Device type names, identifiers, product families

**App Management:**
- **`ListSimulatorApps`** - List installed apps
  - Input: target (UDID or 'booted')
  - Returns: App bundle IDs, names, versions, paths

- **`InstallSimulatorApp`** - Install app on simulator
  - Input: target, app bundle path
  - Returns: Installation status

- **`UninstallSimulatorApp`** - Uninstall app from simulator
  - Input: target, bundle identifier
  - Returns: Uninstall status

- **`LaunchSimulatorApp`** - Launch app on simulator
  - Input: target, bundle identifier
  - Returns: Launch status

- **`TerminateSimulatorApp`** - Terminate running app
  - Input: target, bundle identifier
  - Returns: Termination status

**Simulator Utilities:**
- **`OpenUrlSimulator`** - Open URL in simulator
  - Input: target, URL
  - Returns: Status (useful for deep linking)

- **`ScreenshotSimulator`** - Capture screenshot
  - Input: target, output file path
  - Returns: Screenshot path

- **`GetSimulatorLogs`** - Retrieve simulator logs
  - Input: target, predicate (optional filter), maxLines
  - Returns: Recent log entries (last 5 minutes, up to maxLines)

### Keychain (5 tools)

Manage macOS keychains and certificates.

- **`CreateKeychain`** - Create new keychain
  - Input: name, password
  - Returns: Keychain path

- **`DeleteKeychain`** - Delete a keychain
  - Input: name or path
  - Returns: Deletion confirmation

- **`UnlockKeychain`** - Unlock a keychain
  - Input: password, name (optional, default: login.keychain-db)
  - Returns: Unlock status

- **`SetDefaultKeychain`** - Set default keychain
  - Input: name or path
  - Returns: Status confirmation

- **`ImportPkcs12Keychain`** - Import PKCS12/PFX certificate
  - Input: certificate path, passphrase, keychain name, allowAnyAppRead flag
  - Returns: Import status

## 💡 Usage Examples

### Setting Up a Development Environment

```
AI: "Register my iPhone for development"
→ RegisterDevice(name: "My iPhone 15 Pro", udid: "00008110-...", platform: "IOS")

AI: "Create a bundle ID for my app"
→ CreateBundleId(name: "My App", identifier: "com.mycompany.myapp", platform: "IOS")

AI: "Create a development certificate"
→ CreateCertificate(certificateType: "IOS_DEVELOPMENT", commonName: "My Name", email: "me@company.com")

AI: "Create a development provisioning profile"
→ CreateProvisioningProfile(name: "My App Dev", profileType: "IOS_APP_DEVELOPMENT", ...)
```

### Simulator Testing Workflow

```
AI: "Show me available iPhone simulators"
→ ListSimulators(deviceType: "iPhone")

AI: "Create an iPhone 15 Pro simulator"
→ CreateSimulator(name: "Test iPhone", deviceType: "iPhone 15 Pro")

AI: "Boot the simulator and install my app"
→ BootSimulator(target: "Test iPhone")
→ InstallSimulatorApp(target: "Test iPhone", appPath: "/path/to/MyApp.app")

AI: "Take a screenshot"
→ ScreenshotSimulator(target: "Test iPhone", outputPath: "/tmp/screenshot.png")
```

### CI/CD Provisioning

```
AI: "Set up signing for CI"
→ CreateKeychain(name: "ci-keychain", password: "ci-password")
→ UnlockKeychain(password: "ci-password", name: "ci-keychain")
→ ImportPkcs12Keychain(certificatePath: "/tmp/cert.p12", passphrase: "...", keychain: "ci-keychain")
→ DownloadProvisioningProfile(profileId: "...")
→ InstallProvisioningProfile(base64Data: "...")
```

## 🏗️ Architecture

Built with:
- **[ModelContextProtocol SDK](https://github.com/modelcontextprotocol/specification)** (v0.4.0-preview.3) - Official C# MCP SDK
- **AppleDev Library** - Core Apple tooling wrappers (SimCtl, XCDevice, Keychain, etc.)
- **AppStoreConnectClient** - App Store Connect API integration
- **.NET 9.0** - Modern .NET runtime

### Design Principles

- **Consistent patterns** - All tools follow the same structure
- **Robust error handling** - Platform validation, parameter checking, exception catching
- **Clear output** - Human-readable success/failure messages with context
- **Platform awareness** - macOS-only tools return helpful errors on other platforms
- **Credential flexibility** - Support for multiple Apple Developer accounts

## 🔧 Building and Publishing

### Development Build

```bash
dotnet build AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj
```

### Run for Testing

```bash
# Set environment variables
export APP_STORE_CONNECT_KEY_ID="your-key-id"
export APP_STORE_CONNECT_ISSUER_ID="your-issuer-id"
export APP_STORE_CONNECT_PRIVATE_KEY="$(base64 -i AuthKey.p8 | tr -d '\n')"

# Run the server
dotnet run --project AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj
```

### Production Build

```bash
# Self-contained executable (includes .NET runtime)
dotnet publish AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained true \
  -o ./publish

# Framework-dependent (requires .NET 9.0 installed)
dotnet publish AppleDev.Mcp.Server/AppleDev.Mcp.Server.csproj \
  -c Release \
  -o ./publish
```

## 🤝 Integration

### Compatible MCP Clients

- **Claude Desktop** - Official Anthropic desktop app
- **Cline** - VS Code extension
- **Continue** - VS Code/JetBrains extension
- Any MCP-compatible client

### Capabilities

When connected, AI assistants can:
- ✅ Automate complete iOS/macOS app signing setup
- ✅ Manage devices, certificates, and provisioning profiles
- ✅ Control iOS simulators for testing
- ✅ Extract information from app bundles
- ✅ Set up and tear down CI/CD signing infrastructure
- ✅ Troubleshoot development environment issues
- ✅ Locate and manage Xcode installations

## 📝 Notes

- **macOS Required**: Simulator, device, keychain, and Xcode tools only work on macOS
- **Xcode Required**: Simulator and device tools require Xcode to be installed
- **API Limits**: App Store Connect API has rate limits; the server doesn't implement throttling
- **Security**: Private keys are stored in environment variables; use secure credential management in production
- **Permissions**: Some operations (keychain, simulators) may require macOS permissions

## 📄 License

Same as parent [AppleDev.Tools](https://github.com/Redth/AppleDev.Tools) project.

## 🙏 Credits

Built on top of:
- [AppleDev.Tools](https://github.com/Redth/AppleDev.Tools) by [@Redth](https://github.com/Redth)
- [Model Context Protocol](https://modelcontextprotocol.io) by Anthropic

---

**Status**: ✅ Production Ready - All 44 tools implemented and tested
