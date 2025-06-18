# AppleDev.Tools

.NET Library with useful Apple/Xcode tool wrappers and implementations for developers including a global .NET CLI tool

![image](https://user-images.githubusercontent.com/271950/231289451-0db771e3-c2f6-4b85-a3ea-e80c70439d48.png)

## Table of Contents

- [Installation](#installation)
- [CLI Tool Features](#cli-tool-features)
  - [Simulator Management](#simulator-management)
  - [Device Management](#device-management)
  - [Keychain Operations](#keychain-operations)
  - [Provisioning & Certificates](#provisioning--certificates)
  - [CI/CD Automation](#cicd-automation)
- [Library APIs](#library-apis)
  - [Core Tools](#core-tools)
  - [App Store Connect](#app-store-connect)
  - [Security & Certificates](#security--certificates)
- [Output Formats](#output-formats)
- [Requirements](#requirements)
- [Contributing](#contributing)
- [License](#license)

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
List, create, boot, shutdown, delete, erase, and screenshot simulators:
```bash
apple simulator list --available --runtime "iOS 18.4"
apple simulator create "My iPhone 16" --device-type "iPhone 16"
apple simulator boot "My iPhone 16" --wait
apple simulator screenshot "My iPhone 16" --output screenshot.png
```

### Device Management
Enumerate and filter connected iOS, watchOS, tvOS, and visionOS devices:
```bash
apple device list --format json --verbose
```

### Keychain Operations
Create, import certificates, and manage keychains:
```bash
apple keychain create --keychain build.keychain --password buildpass
apple keychain import ios-cert.p12 --keychain build.keychain
apple keychain unlock --keychain build.keychain --password buildpass
```

### Provisioning & Certificates
Manage provisioning profiles, bundle IDs, and certificates:
```bash
apple provisioning list --download
apple bundleids list
apple certificate create --output ~/certificates/
```

### CI/CD Automation
Streamlined commands for continuous integration:
```bash
# Complete CI provisioning
apple ci provision --certificate $IOS_CERT_BASE64 --bundle-identifier com.myapp.id

# Create and decode secrets
apple ci secret --from-certificate ios-distribution.p12
apple ci base64-to-file --base64 $CERT_BASE64 --output-file cert.p12
```

## Library APIs

### Core Tools
- **Xcode**: Locate installations and get version information
- **XCRun**: Find command line tools and manage AuthKeys
- **XCDevice**: List connected devices with real-time monitoring
- **SimCtl**: Complete simulator management and app lifecycle control
- **ALTool**: Upload and validate apps to App Store Connect

### App Store Connect
- **AppStoreConnectClient**: Complete API wrapper for provisioning profiles, certificates, bundle IDs, and device registration with filtering and automatic installation support.

### Security & Certificates
- **Keychain**: Create, manage, and import certificates to keychains
- **ProvisioningProfiles**: Parse and install .mobileprovision files
- **AppBundleReader**: Extract metadata from .app bundles
- **CertificateRequestGenerator**: Generate CSRs and RSA key pairs

## Output Formats
All commands support **Table** (default), **JSON**, and **Verbose** output formats.

## Requirements
- **macOS**: Required for Xcode-dependent features
- **.NET 8.0+**: Required runtime
- **Xcode**: Required for simulator/device management
- **App Store Connect API Key**: Required for ASC integration

## Contributing
Contributions welcome! Submit issues, feature requests, or pull requests.

## License
See the [LICENSE](LICENSE) file for license information.
