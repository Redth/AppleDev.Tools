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
  - [App Store Connect](#app-store-connect)
  - [Xcode Tools](#xcode-tools)
  - [App Management](#app-management)
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
 
Manage iOS, watchOS, tvOS, and visionOS simulators.

#### `apple simulator list`
Lists simulators with powerful filtering options.

**Options:**
- `--booted` - Show only running simulators
- `--available` - Show only available simulators
- `--unavailable` - Show only unavailable simulators (e.g., unsupported runtime)
- `--name <name>` - Filter by simulator name
- `--udid <udid>` - Filter by UDID
- `--runtime <runtime>` - Filter by runtime (e.g., "iOS 18.3", "tvOS 18.2")
- `--device-type <type>` - Filter by device type (e.g., "iPhone 16 Pro")
- `--product-family <family>` - Filter by product family (e.g., "iPhone", "Apple TV")
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details
- `--include-screen-info` - Include device type screen information (size, scale, DPI, pixel dimensions, model identifier)

**Examples:**
```bash
apple simulator list
apple simulator list --available --runtime "iOS 18.4"
apple simulator list --booted --format json
apple simulator list --product-family iPhone
```

#### `apple simulator create <name>`
Creates a new simulator.

**Arguments:**
- `<name>` - Name for the new simulator

**Options:**
- `-d, --device-type <type>` - Device type identifier (required, e.g., "iPhone 15")
- `-r, --runtime <runtime>` - Runtime identifier (optional, uses newest compatible if omitted)

**Examples:**
```bash
apple simulator create "My iPhone 16" --device-type "iPhone 16"
apple simulator create "Test Device" --device-type "iPhone 15 Pro" --runtime "iOS 17.0"
```

#### `apple simulator boot <target>`
Boots a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name

**Options:**
- `--wait` - Wait until simulator is fully booted
- `--timeout <seconds>` - Timeout for wait (default: 120)

**Examples:**
```bash
apple simulator boot "My iPhone 16"
apple simulator boot --wait --timeout 180 ABCD1234-1234-1234-123456789ABC
```

#### `apple simulator shutdown <target>`
Shuts down running simulators.

**Arguments:**
- `<target>` - Simulator UDID, name, "booted", or "all"

**Examples:**
```bash
apple simulator shutdown "My iPhone 16"
apple simulator shutdown booted
apple simulator shutdown all
```

#### `apple simulator erase <target>`
Erases simulator content and settings (factory reset).

**Arguments:**
- `<target>` - Simulator UDID, name, "booted", or "all"

**Examples:**
```bash
apple simulator erase "My iPhone 16"
apple simulator erase all
```

#### `apple simulator delete <target>`
Permanently deletes simulators.

**Arguments:**
- `<target>` - Simulator UDID, name, "unavailable", or "all"

**Examples:**
```bash
apple simulator delete "Old Test Device"
apple simulator delete unavailable
apple simulator delete all
```

#### `apple simulator screenshot <target>`
Captures a screenshot from a running simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `-o, --output <path>` - Output file or directory path

**Examples:**
```bash
apple simulator screenshot "My iPhone 16"
apple simulator screenshot --output ~/Desktop/screenshot.png "My iPhone 16"
```

#### `apple simulator app install <target> <app-path>`
Installs an app (.app bundle) on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<app-path>` - Path to .app bundle

**Examples:**
```bash
apple simulator app install "My iPhone 16" ~/MyApp.app
apple simulator app install booted /path/to/MyApp.app
```

#### `apple simulator app uninstall <target> <bundle-id>`
Uninstalls an app from a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<bundle-id>` - Bundle identifier of the app

**Examples:**
```bash
apple simulator app uninstall "My iPhone 16" com.mycompany.myapp
apple simulator app uninstall booted com.example.app
```

#### `apple simulator app launch <target> <bundle-id>`
Launches an installed app on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<bundle-id>` - Bundle identifier of the app

**Examples:**
```bash
apple simulator app launch "My iPhone 16" com.mycompany.myapp
apple simulator app launch booted com.example.app
```

#### `apple simulator app terminate <target> <bundle-id>`
Terminates a running app on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<bundle-id>` - Bundle identifier of the app

**Examples:**
```bash
apple simulator app terminate "My iPhone 16" com.mycompany.myapp
apple simulator app terminate booted com.example.app
```

#### `apple simulator app list <target>`
Lists all installed apps on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple simulator app list "My iPhone 16"
apple simulator app list booted --format json
apple simulator app list --verbose
```

#### `apple simulator open [udid]`
Opens the Simulator.app, optionally to a specific simulator.

**Arguments:**
- `[udid]` - Optional simulator UDID (if omitted, just opens Simulator.app)

**Examples:**
```bash
apple simulator open
apple simulator open ABCD1234-1234-1234-123456789ABC
```

#### `apple simulator open-url <target> <url>`
Opens a URL on a simulator (for deep linking and URL scheme testing).

**Arguments:**
- `<target>` - Simulator UDID or name
- `<url>` - URL to open

**Examples:**
```bash
apple simulator open-url "My iPhone 16" "myapp://profile/123"
apple simulator open-url booted "https://example.com"
```

#### `apple simulator logs <target>`
Retrieves and displays logs from a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `--predicate <predicate>` - NSPredicate filter for logs (e.g., "eventMessage contains 'error'")
- `--start <timestamp>` - Start timestamp for logs (e.g., "2025-10-30 10:00:00")
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple simulator logs "My iPhone 16"
apple simulator logs booted --predicate "eventMessage contains 'error'"
apple simulator logs "My iPhone 16" --start "2025-10-30 10:00:00"
apple simulator logs "My iPhone 16" --format json
apple simulator logs booted --verbose
```

#### `apple simulator device-types`
Lists available simulator device types and runtime versions with optional screen information.

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details
- `--include-screen-info` - Include device type screen information (size, scale, DPI, pixel dimensions, model identifier, product class)

**Screen Information:**
When `--include-screen-info` is specified, the following additional columns are shown:
- Screen Size - Width and height in points (e.g., "1206x2622")
- Scale - Display scale factor (e.g., "3" for @3x Retina displays)
- Pixel Size - Physical pixel dimensions (calculated as width * scale)
- DPI - Display dots per inch for width and height
- Model Identifier - Device model code (e.g., "iPhone17,1")
- Product Class - Product class code (e.g., "D93")
- Colorspace - Display colorspace (e.g., "DisplayP3" for Vision Pro)

**Examples:**
```bash
apple simulator device-types
apple simulator device-types --include-screen-info
apple simulator device-types --format json
apple simulator device-types --verbose
```

### Device Management

#### `apple simulator create <name>`
Creates a new simulator.

**Arguments:**
- `<name>` - Name for the new simulator

**Options:**
- `-d, --device-type <type>` - Device type identifier (required, e.g., "iPhone 15")
- `-r, --runtime <runtime>` - Runtime identifier (optional, uses newest compatible if omitted)

**Examples:**
```bash
apple simulator create "My iPhone 16" --device-type "iPhone 16"
apple simulator create "Test Device" --device-type "iPhone 15 Pro" --runtime "iOS 17.0"
```

#### `apple simulator boot <target>`
Boots a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name

**Options:**
- `--wait` - Wait until simulator is fully booted
- `--timeout <seconds>` - Timeout for wait (default: 120)

**Examples:**
```bash
apple simulator boot "My iPhone 16"
apple simulator boot --wait --timeout 180 ABCD1234-1234-1234-1234-123456789ABC
```

#### `apple simulator shutdown <target>`
Shuts down running simulators.

**Arguments:**
- `<target>` - Simulator UDID, name, "booted", or "all"

**Examples:**
```bash
apple simulator shutdown "My iPhone 16"
apple simulator shutdown booted
apple simulator shutdown all
```

#### `apple simulator erase <target>`
Erases simulator content and settings (factory reset).

**Arguments:**
- `<target>` - Simulator UDID, name, "booted", or "all"

**Examples:**
```bash
apple simulator erase "My iPhone 16"
apple simulator erase all
```

#### `apple simulator delete <target>`
Permanently deletes simulators.

**Arguments:**
- `<target>` - Simulator UDID, name, "unavailable", or "all"

**Examples:**
```bash
apple simulator delete "Old Test Device"
apple simulator delete unavailable
```

#### `apple simulator screenshot <target>`
Captures a screenshot from a running simulator.

**Arguments:**
- `<target>` - Simulator UDID or name

**Options:**
- `-o, --output <path>` - Output file or directory path

**Examples:**
```bash
apple simulator screenshot "My iPhone 16"
apple simulator screenshot --output ~/Desktop/screenshot.png "My iPhone 16"
```

#### `apple simulator app install <target> <app-path>`
Installs an app (.app bundle) on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<app-path>` - Path to .app bundle

**Examples:**
```bash
apple simulator app install "My iPhone 16" ~/MyApp.app
apple simulator app install booted /path/to/MyApp.app
```

#### `apple simulator app uninstall <target> <bundle-id>`
Uninstalls an app from a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<bundle-id>` - Bundle identifier of the app

**Examples:**
```bash
apple simulator app uninstall "My iPhone 16" com.mycompany.myapp
apple simulator app uninstall booted com.example.app
```

#### `apple simulator app launch <target> <bundle-id>`
Launches an installed app on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<bundle-id>` - Bundle identifier of the app

**Examples:**
```bash
apple simulator app launch "My iPhone 16" com.mycompany.myapp
apple simulator app launch booted com.example.app
```

#### `apple simulator app terminate <target> <bundle-id>`
Terminates a running app on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name
- `<bundle-id>` - Bundle identifier of the app

**Examples:**
```bash
apple simulator app terminate "My iPhone 16" com.mycompany.myapp
apple simulator app terminate booted com.example.app
```

#### `apple simulator app list <target>`
Lists all installed apps on a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple simulator app list "My iPhone 16"
apple simulator app list booted --format json
```

#### `apple simulator open [udid]`
Opens the Simulator.app, optionally to a specific simulator.

**Arguments:**
- `[udid]` - Optional simulator UDID (if omitted, just opens Simulator.app)

**Examples:**
```bash
apple simulator open
apple simulator open ABCD1234-1234-1234-1234-123456789ABC
```

#### `apple simulator device-types`
Lists available simulator device types and runtimes.

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple simulator device-types
apple simulator device-types --format json
```

#### `apple simulator open-url <target> <url>`
Opens a URL on a simulator (for deep linking and URL scheme testing).

**Arguments:**
- `<target>` - Simulator UDID or name
- `<url>` - URL to open

**Examples:**
```bash
apple simulator open-url "My iPhone 16" "myapp://profile/123"
apple simulator open-url booted "https://example.com"
```

#### `apple simulator logs <target>`
Retrieves and displays logs from a simulator.

**Arguments:**
- `<target>` - Simulator UDID or name

**Options:**
- `--predicate <predicate>` - NSPredicate filter for logs (e.g., "eventMessage contains 'error'")
- `--start <timestamp>` - Start timestamp for logs (e.g., "2025-10-30 10:00:00")
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple simulator logs "My iPhone 16"
apple simulator logs booted --predicate "eventMessage contains 'error'"
apple simulator logs booted --start "2025-10-30 10:00:00"
apple simulator logs "My iPhone 16" --format json
```

### Device Management

#### `apple device list`
Lists connected iOS, watchOS, tvOS, and visionOS devices.

**Options:**
- `-t, --timeout <seconds>` - Timeout to search for network devices (default: 5)
- `-d, --devices-only` - Show devices only, exclude simulators
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple device list
apple device list --devices-only --format json
apple device list --timeout 10 --verbose
```

### Keychain Operations

#### `apple keychain create`
Creates a new keychain file.

**Options:**
- `-k, --keychain <keychain>` - Keychain path (required)
- `-p, --password <password>` - Keychain password (required)

**Examples:**
```bash
apple keychain create --keychain build.keychain --password buildpass
apple keychain create --keychain ~/Library/Keychains/ci-temp.keychain-db --password temp1234
```

#### `apple keychain delete`
Deletes a keychain file.

**Options:**
- `-k, --keychain <keychain>` - Keychain path (required)

**Examples:**
```bash
apple keychain delete --keychain ~/Library/Keychains/ci-temp.keychain-db
```

#### `apple keychain unlock`
Unlocks a keychain for automated access.

**Options:**
- `-k, --keychain <keychain>` - Keychain path (default: login.keychain-db)
- `-p, --password <password>` - Keychain password (required)

**Examples:**
```bash
apple keychain unlock --keychain build.keychain --password buildpass
apple keychain unlock --keychain ~/Library/Keychains/login.keychain-db --password mypass
```

#### `apple keychain import <certificate_file>`
Imports a PKCS#12 certificate (.p12/.pfx) into a keychain.

**Arguments:**
- `<certificate_file>` - Path to certificate file

**Options:**
- `-k, --keychain <keychain>` - Keychain path (default: login.keychain-db)
- `-p, --passphrase <passphrase>` - Certificate passphrase
- `--allow-any-app-read` - Allow any app to read the certificate

**Examples:**
```bash
apple keychain import ios-cert.p12 --keychain build.keychain
apple keychain import ~/certificates/distribution.pfx --passphrase certpass --allow-any-app-read
```

#### `apple keychain default`
Sets the default keychain for the current user.

**Options:**
- `-k, --keychain <keychain>` - Keychain path (required)

**Examples:**
```bash
apple keychain default --keychain ~/Library/Keychains/login.keychain-db
apple keychain default --keychain build.keychain
```

### Provisioning Profiles

#### `apple provisioning list`
Lists App Store Connect provisioning profiles.

**Options:**
- `-t, --type <type>` - Filter by profile type (e.g., IOS_APP_DEVELOPMENT, IOS_APP_STORE)
- `-a, --active` - Show only active profiles
- `-d, --download` - Download and install profiles locally
- `--download-path <directory>` - Override default download directory
- `-b, --bundle-id <bundle-id>` - Filter by bundle identifier
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple provisioning list --active
apple provisioning list --download --type IOS_APP_DEVELOPMENT
apple provisioning list --bundle-id com.myapp.* --format json
```

#### `apple provisioning installed`
Lists locally installed provisioning profiles.

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple provisioning installed
apple provisioning installed --format json --verbose
```

#### `apple provisioning parse <file>`
Parses and displays information from a .mobileprovision file.

**Arguments:**
- `<file>` - Path to .mobileprovision file

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple provisioning parse MyApp_Development.mobileprovision
apple provisioning parse ~/Downloads/AppStore.mobileprovision --format json
```

#### `apple provisioning create`
Creates a new provisioning profile via App Store Connect.

**Options:**
- `--name <name>` - Name for the new provisioning profile (required)
- `--type <type>` - Profile type (required): IOS_APP_DEVELOPMENT, IOS_APP_STORE, IOS_APP_ADHOC, etc.
- `--bundle-id <id>` - Bundle identifier ID from App Store Connect (required)
- `--certificates <ids>` - Certificate ID(s), comma-separated (required)
- `--devices <ids>` - Device ID(s), comma-separated (optional, for specific devices)
- `--all-devices` - Include all enabled devices for the platform (alternative to --devices)
- `--download` - Download the profile after creation
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
# Create dev profile with specific devices
apple provisioning create \
  --name "My Dev Profile" \
  --type IOS_APP_DEVELOPMENT \
  --bundle-id BNDL123 \
  --certificates CERT456 \
  --devices DEV789,DEV012

# Create dev profile with ALL enabled devices
apple provisioning create \
  --name "My Dev Profile" \
  --type IOS_APP_DEVELOPMENT \
  --bundle-id BNDL123 \
  --certificates CERT456 \
  --all-devices

# Create App Store profile (no devices needed)
apple provisioning create \
  --name "App Store Profile" \
  --type IOS_APP_STORE \
  --bundle-id BNDL123 \
  --certificates CERT789 \
  --download
```

#### `apple provisioning delete <id>`
Deletes a provisioning profile from App Store Connect.

**Arguments:**
- `<id>` - Provisioning profile ID

**Options:**
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple provisioning delete PROF123456
```

### Bundle IDs

#### `apple bundleids list`
Lists App Store Connect bundle identifiers (app IDs).

**Options:**
- `--platform <platform>` - Filter by platform (IOS, MAC_OS)
- `--id <id>` - Filter by ID
- `--identifier <identifier>` - Filter by identifier pattern
- `--seed-id <seedid>` - Filter by seed ID
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple bundleids list
apple bundleids list --platform IOS --format json
apple bundleids list --identifier com.mycompany.*
```

#### `apple bundleids create <name> <identifier> <platform>`
Creates a new bundle identifier (app ID) in App Store Connect.

**Arguments:**
- `<name>` - Display name for the bundle ID
- `<identifier>` - Bundle identifier (e.g., com.mycompany.myapp)
- `<platform>` - Platform (IOS or MAC_OS)

**Options:**
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple bundleids create "My App" com.mycompany.myapp IOS
apple bundleids create "My Mac App" com.mycompany.macapp MAC_OS
```

#### `apple bundleids update <id>`
Updates an existing bundle identifier in App Store Connect.

**Arguments:**
- `<id>` - Bundle identifier ID (from App Store Connect)

**Options:**
- `--name <name>` - New display name (required)
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple bundleids update BUNDLEID123 --name "Updated App Name"
```

#### `apple bundleids delete <id>`
Deletes a bundle identifier from App Store Connect.

**Arguments:**
- `<id>` - Bundle identifier ID (from App Store Connect)

**Options:**
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple bundleids delete BUNDLEID123
```

### Certificates

#### `apple certificate create`
Creates a new signing certificate via App Store Connect.

**Options:**
- `-t, --type <type>` - Certificate type: DEVELOPMENT, DISTRIBUTION, etc. (default: DEVELOPMENT)
- `-c, --common-name <name>` - Common name for CSR (optional)
- `--passphrase <passphrase>` - Passphrase for output .pfx file (optional)
- `-o, --output <path>` - Output path (file or directory)
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple certificate create --type DISTRIBUTION
apple certificate create --output ~/certificates/ --type DEVELOPMENT
apple certificate create --type IOS_DISTRIBUTION --passphrase mypass
```

#### `apple certificate list`
Lists signing certificates from App Store Connect.

**Options:**
- `-t, --type <type>` - Filter by certificate type (IOS_DEVELOPMENT, IOS_DISTRIBUTION, etc.)
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple certificate list
apple certificate list --type IOS_DISTRIBUTION --format json
```

#### `apple certificate revoke <id>`
Revokes a certificate in App Store Connect.

**Arguments:**
- `<id>` - Certificate ID (from App Store Connect)

**Options:**
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple certificate revoke CERT123456
```

### App Store Connect

App Store Connect commands for managing devices, bundle IDs, certificates, and provisioning profiles.

#### `apple devices list`
Lists registered devices from App Store Connect.

**Options:**
- `--name <name>` - Filter by device name
- `--platform <platform>` - Filter by platform (IOS, MAC_OS)
- `--status <status>` - Filter by status (ENABLED, DISABLED)
- `--udid <udid>` - Filter by UDID
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple devices list
apple devices list --platform IOS --status ENABLED
apple devices list --format json
```

#### `apple devices register <name> <udid> <platform>`
Registers a new device with App Store Connect.

**Arguments:**
- `<name>` - Device name
- `<udid>` - Device UDID
- `<platform>` - Platform (IOS or MAC_OS)

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple devices register "My iPhone" 00008030-001234567890ABCD IOS
apple devices register "My Mac" ABCD1234-5678-90AB-CDEF-1234567890AB MAC_OS
```

#### `apple devices update <id>`
Updates a registered device in App Store Connect.

**Arguments:**
- `<id>` - Device ID (from App Store Connect)

**Options:**
- `-n, --name <name>` - New device name
- `-s, --status <status>` - New status (ENABLED or DISABLED)
- `--format <format>` - Output format: table (default), json, or verbose
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple devices update DEV123456 --name "Updated iPhone Name"
apple devices update DEV123456 --status DISABLED
```

### Xcode Tools

#### `apple xcode list`
Lists all installed Xcode versions.

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple xcode list
apple xcode list --format json
```

#### `apple xcode locate`
Finds the currently selected Xcode installation.

**Options:**
- `--best` - Show best available (highest version if none selected)
- `--format <format>` - Output format: table (default), json, or verbose

**Examples:**
```bash
apple xcode locate
apple xcode locate --best --format json
```

### App Management

#### `apple app-info <app-path>`
Reads and displays Info.plist metadata from an app bundle.

**Arguments:**
- `<app-path>` - Path to .app bundle

**Options:**
- `--format <format>` - Output format: table (default), json, or verbose
- `--verbose` - Show additional details

**Examples:**
```bash
apple app-info ~/MyApp.app
apple app-info /path/to/MyApp.app --format json
```

#### `apple upload <app-path>`
Uploads an app to App Store Connect / TestFlight.

**Arguments:**
- `<app-path>` - Path to .app or .ipa file

**Options:**
- `-t, --type <type>` - App type (required): ios, macos, watchos, tvos, visionos
- `--format <format>` - Output format: table (default), json, or verbose
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple upload ~/MyApp.ipa --type ios
apple upload ~/MyMacApp.app --type macos
```

#### `apple validate <app-path>`
Validates an app before uploading to App Store Connect.

**Arguments:**
- `<app-path>` - Path to .app or .ipa file

**Options:**
- `-t, --type <type>` - App type (required): ios, macos, watchos, tvos, visionos
- `--format <format>` - Output format: table (default), json, or verbose
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
apple validate ~/MyApp.ipa --type ios
apple validate ~/MyMacApp.app --type macos --format json
```

### CI/CD Automation

Streamlined commands for continuous integration environments.

#### `apple ci provision`
Sets up CI environment with certificates, keychain, and provisioning profiles.

**Options:**
- `--certificate <value>` - Base64 certificate data, file path, or environment variable name
- `--certificate-passphrase <passphrase>` - Certificate passphrase
- `--keychain <keychain>` - Keychain name (default: "build")
- `--keychain-password <password>` - Keychain password (default: same as keychain name)
- `--keychain-allow-any-app-read` - Allow any app read permission (default: true)
- `--keychain-disallow-any-app-read` - Disallow any app read permission
- `--bundle-identifier <id>` - Bundle identifier(s) to match profiles for (can specify multiple)
- `--profile-type <type>` - Provisioning profile type(s) to download (can specify multiple)
- `--profile-path <directory>` - Override default profile install directory
- `--install-api-private-key` - Install App Store Connect private key to disk
- `--api-private-key-dir <directory>` - Directory for API private key (default: ~/private_keys/)
- App Store Connect API options (see [App Store Connect Authentication](#app-store-connect-authentication))

**Examples:**
```bash
# Basic certificate import
apple ci provision --certificate $IOS_CERT_BASE64 --certificate-passphrase $CERT_PASS

# Full CI provisioning with profiles
apple ci provision \
  --certificate $IOS_CERT_BASE64 \
  --bundle-identifier com.myapp.id \
  --api-key-id $ASC_KEY_ID \
  --api-issuer-id $ASC_ISSUER_ID \
  --api-private-key $ASC_PRIVATE_KEY

# Multiple bundle IDs and profile types
apple ci provision \
  --certificate cert.p12 \
  --bundle-identifier com.myapp.main \
  --bundle-identifier com.myapp.widget \
  --profile-type IOS_APP_DEVELOPMENT \
  --profile-type IOS_APP_ADHOC
```

#### `apple ci deprovision`
Cleans up CI environment (removes temporary keychain).

**Options:**
- `--keychain <keychain>` - Keychain to delete (default: "build")

**Examples:**
```bash
apple ci deprovision
apple ci deprovision --keychain ci-build.keychain
```

#### `apple ci secret`
Converts certificates/keys to base64 for secure CI storage.

**Options:**
- `--from-certificate <file>` - Certificate file (.p12/.pfx)
- `--from-private-key <file>` - Private key file (.p8)
- `--from-keystore <file>` - Keystore file (.keystore)
- `--from-pepk <file>` - Google PEPK key file (.pepk)
- `--from-text-file <file>` - Text file
- `--from-binary-file <file>` - Binary file
- `--output-file <file>` - Save output to file

**Examples:**
```bash
apple ci secret --from-certificate ios-distribution.p12
apple ci secret --from-private-key AuthKey_ABC123.p8 --output-file key.txt
apple ci secret --from-certificate cert.pfx > secret.txt
```

#### `apple ci base64-to-file`
Decodes base64 data to file (from environment variable or string).

**Options:**
- `--base64 <value>` - Base64 string or environment variable name (required)
- `--output-file <file>` - Output file path (required)

**Examples:**
```bash
apple ci base64-to-file --base64 $IOS_CERT_BASE64 --output-file cert.p12
apple ci base64-to-file --base64 "aGVsbG8=" --output-file decoded.txt
```

#### `apple ci env-to-file`
Writes environment variable value directly to file.

**Options:**
- `-e, --env-var <name>` - Environment variable name (required)
- `--decode-base64` - Decode base64 before writing
- `--output-file <file>` - Output file path (required)

**Examples:**
```bash
apple ci env-to-file --env-var APP_STORE_CONNECT_API_KEY --output-file AuthKey.p8
apple ci env-to-file --env-var CERT_BASE64 --decode-base64 --output-file cert.p12
```

### App Store Connect Authentication

Commands that interact with App Store Connect require API credentials:

**Options:**
- `--api-key-id <key_id>` - App Store Connect Key ID (or set `APP_STORE_CONNECT_KEY_ID` env var)
- `--api-issuer-id <issuer_id>` - App Store Connect Issuer ID (or set `APP_STORE_CONNECT_ISSUER_ID` env var)
- `--api-private-key <key>` - Private key (.p8) file path, contents, or env var name (or set `APP_STORE_CONNECT_PRIVATE_KEY` env var)

**Example:**
```bash
export APP_STORE_CONNECT_KEY_ID="ABC123DEF4"
export APP_STORE_CONNECT_ISSUER_ID="12345678-1234-1234-1234-123456789012"
export APP_STORE_CONNECT_PRIVATE_KEY="$(cat AuthKey_ABC123DEF4.p8)"

apple provisioning list --active
apple bundleids list
apple certificate create
```

### Output Formats

Most commands support multiple output formats via the `--format` option:

- **table** (default) - Human-readable table format
- **json** - JSON output for scripting and automation
- **verbose** - Additional `--verbose` flag shows extended information

**Example:**
```bash
apple simulator list --format json
apple device list --verbose
apple provisioning list --format json --verbose
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

## Requirements
- **macOS**: Required for Xcode-dependent features
- **.NET 8.0+**: Required runtime
- **Xcode**: Required for simulator/device management
- **App Store Connect API Key**: Required for ASC integration

## Contributing
Contributions welcome! Submit issues, feature requests, or pull requests.

### Running Tests

The test suite includes tests for App Store Connect API integration. To run these tests:

1. Copy `.env.example` to `.env` in the project root:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and add your App Store Connect API credentials:
   ```
   APP_STORE_CONNECT_KEY_ID=your_key_id_here
   APP_STORE_CONNECT_ISSUER_ID=your_issuer_id_here
   APP_STORE_CONNECT_PRIVATE_KEY=your_base64_encoded_private_key_here
   ```

3. Run the tests:
   ```bash
   dotnet test
   ```

Tests that require credentials will be automatically skipped if the `.env` file is not present or credentials are not configured. See [AppleDev.Test/README.md](AppleDev.Test/README.md) for more details.

## License
See the [LICENSE](LICENSE) file for license information.
