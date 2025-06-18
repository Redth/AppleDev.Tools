# AppleDev.Tools

.NET Library with useful Apple/Xcode tool wrappers and implementations for developers including a global .NET CLI tool

![image](https://user-images.githubusercontent.com/271950/231289451-0db771e3-c2f6-4b85-a3ea-e80c70439d48.png)

## Installation

Install the global CLI tool:
```bash
dotnet tool install -g AppleDev.Tool
```

Install the library package:
```bash
dotnet add package AppleDev
```

## CLI Tool Features

The `apple` command provides comprehensive tooling for Apple development workflows:

### Simulator Management
- **List**: Filter simulators by availability, runtime, device type, or name
- **Create**: Create new simulators with custom configurations
- **Boot**: Start simulators with optional wait-for-ready functionality
- **Shutdown**: Stop running simulators (individual, all, or by status)
- **Delete**: Permanently remove simulators
- **Erase**: Factory reset simulator content and settings
- **Screenshot**: Capture screenshots from running simulators

**Examples:**
```bash
apple simulator list --available --runtime "iOS 18.4"
apple simulator create "My iPhone 16" --device-type "iPhone 16"
apple simulator boot "My iPhone 16" --wait
apple simulator screenshot "My iPhone 16" --output screenshot.png
apple simulator shutdown "My iPhone 16"
apple simulator delete "My iPhone 16"
```

### Device Management
- **List**: Enumerate connected iOS, watchOS, tvOS, and visionOS devices
- **Filter**: View devices with detailed information in multiple output formats

**Examples:**
```bash
apple device list
apple device list --format json --verbose
```

### Keychain Operations
- **Create**: Generate new keychain files with passwords
- **Delete**: Remove keychain files securely
- **Import**: Add PKCS#12 certificates (.p12/.pfx) to keychains
- **Unlock**: Enable automated access to keychain contents
- **Default**: Set the default keychain for the user

**Examples:**
```bash
apple keychain create --keychain build.keychain --password buildpass
apple keychain import ios-cert.p12 --keychain build.keychain
apple keychain unlock --keychain build.keychain --password buildpass
```

### Provisioning Profile Management
- **List**: View App Store Connect provisioning profiles with download option
- **Installed**: Show locally installed profiles on the system
- **Parse**: Extract and display information from .mobileprovision files

**Examples:**
```bash
apple provisioning list --download
apple provisioning installed --format json
apple provisioning parse MyApp.mobileprovision
```

### Bundle ID Management
- **List**: Enumerate App Store Connect bundle identifiers (App IDs)

**Examples:**
```bash
apple bundleids list
apple bundleids list --format json --verbose
```

### Certificate Management
- **Create**: Generate new signing certificates via App Store Connect

**Examples:**
```bash
apple certificate create --output ~/certificates/
```

### CI/CD Automation
Streamlined commands for continuous integration environments:

- **Provision**: Complete CI setup with certificates, keychain, and provisioning profiles
- **Deprovision**: Clean up temporary CI resources
- **Secret**: Convert certificates/keys to base64 for secure storage
- **Base64-to-File**: Decode base64 environment variables to files
- **Env-to-File**: Write environment variables directly to files

**Examples:**
```bash
# Complete CI provisioning
apple ci provision --certificate $IOS_CERT_BASE64 --bundle-identifier com.myapp.id

# Create secrets for CI storage
apple ci secret --from-certificate ios-distribution.p12

# Decode secrets in CI
apple ci base64-to-file --base64 $CERT_BASE64 --output-file cert.p12

# Clean up CI environment
apple ci deprovision --keychain ci-build.keychain
```

## Library APIs

### Core Apple Development Tools

#### Xcode
- **Locate**: Find Xcode installations and get version information
- **Path Detection**: Automatically discover Xcode.app locations

#### XCRun
- **Tool Location**: Find and execute Xcode command line tools
- **AuthKey Management**: Install App Store Connect API keys (AuthKey_*.p8 files)

#### XCDevice
- **Device Enumeration**: List all connected devices and simulators
- **Device Observation**: Monitor device connection/disconnection events in real-time
- **Filtering**: Filter by connection type (USB, WiFi, or both)
- **Timeout Control**: Configure discovery timeouts for performance optimization

#### SimCtl (Simulator Control)
- **Simulator Management**: List, create, boot, shutdown, delete, and erase simulators
- **App Lifecycle**: Install, uninstall, launch, and terminate apps in simulators
- **Media Management**: Add photos, videos, and other media to simulator libraries
- **Interaction**: Open URLs, take screenshots, and control simulator state
- **Simulator.app Integration**: Open and control the Simulator application

#### ALTool (Application Loader)
- **App Upload**: Upload apps to App Store Connect and TestFlight
- **App Validation**: Validate app bundles before submission
- **Multi-Platform Support**: iOS, macOS, watchOS, and tvOS applications

### App Store Connect Integration

#### AppStoreConnectClient
Complete App Store Connect API wrapper with support for:

- **Provisioning Profiles**: List, create, download, and install provisioning profiles
- **Certificates**: List, create, and revoke development/distribution certificates
- **Bundle IDs**: List and manage App Store Connect bundle identifiers
- **Device Registration**: Register development devices for testing
- **Filtering & Pagination**: Advanced querying with comprehensive filter options
- **Automatic Installation**: Direct integration with local provisioning profile management

### Security & Certificate Management

#### Keychain
- **Keychain Lifecycle**: Create, delete, and manage keychain files
- **Certificate Import**: Import PKCS#12 certificates with proper access controls
- **Security Integration**: Unlock keychains and set default keychain preferences
- **Partition Management**: Configure keychain access for automated tools

#### ProvisioningProfiles
- **Profile Parsing**: Extract detailed information from .mobileprovision files
- **Installation Management**: Install profiles to appropriate system locations
- **Multi-Platform Support**: Handle both iOS (.mobileprovision) and macOS (.provisionprofile) profiles
- **Version Compatibility**: Support for Xcode 16+ directory structures

#### AppBundleReader
- **Bundle Analysis**: Read and parse Info.plist files from .app bundles
- **Metadata Extraction**: Extract version numbers, bundle identifiers, and platform information
- **Cross-Platform Support**: Handle both iOS and macOS app bundle structures

### Certificate Generation

#### CertificateRequestGenerator
- **CSR Generation**: Create certificate signing requests programmatically
- **Automatic Key Generation**: Generate RSA key pairs for certificate requests
- **PEM Format Support**: Output certificates in standard PEM format

## Output Formats

All CLI commands support multiple output formats:
- **Table**: Human-readable tabular output (default)
- **JSON**: Machine-readable JSON for scripting and automation
- **Verbose**: Detailed information with additional metadata

## Platform Support

- **macOS**: Full feature support (required for Xcode integration)
- **Cross-Platform**: Core library features available on Windows and Linux for CI/CD scenarios

## Requirements

- **macOS**: Required for Xcode-dependent features (simulators, devices, code signing)
- **.NET 8.0+**: Required runtime version
- **Xcode**: Required for simulator and device management features
- **App Store Connect API Key**: Required for App Store Connect integration features

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## License

See the [LICENSE](LICENSE) file for license information.
