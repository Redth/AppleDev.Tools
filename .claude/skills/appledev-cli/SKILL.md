---
name: appledev-cli
description: >
  Reference for the AppleDev.Tools CLI (`apple` command) - a .NET global tool for Apple development automation.
  Use when: (1) Managing iOS/macOS simulators (list, create, boot, screenshot, install apps),
  (2) Working with App Store Connect (certificates, profiles, bundle IDs, devices),
  (3) Setting up CI/CD for Apple development (provision, deprovision, keychain management),
  (4) Uploading apps to TestFlight/App Store Connect,
  (5) Parsing or managing provisioning profiles locally.
  Covers all commands, options, and common workflows.
---

# AppleDev.Tools CLI Reference

## Installation

```bash
dotnet tool install -g AppleDev.Tool
```

The tool installs as the `apple` command.

## Authentication for App Store Connect Commands

Commands that interact with App Store Connect require authentication via environment variables or command options:

```bash
# Environment variables (recommended)
export APP_STORE_CONNECT_KEY_ID="ABC123DEF4"
export APP_STORE_CONNECT_ISSUER_ID="12345678-1234-1234-1234-123456789012"
export APP_STORE_CONNECT_PRIVATE_KEY="$(cat ~/AuthKey_ABC123DEF4.p8)"

# Or pass directly to commands
apple provisioning list \
  --key-id ABC123DEF4 \
  --issuer-id 12345678-1234-1234-1234-123456789012 \
  --private-key ~/AuthKey_ABC123DEF4.p8
```

## Output Formats

Most commands support `--format` option:
- `None` (default) - Human-readable table
- `json` - JSON output for scripting
- `--verbose` - Additional details

---

# Simulator Commands

Manage iOS, watchOS, tvOS, and visionOS simulators.

## List Simulators

```bash
apple simulator list                           # All simulators
apple simulator list --booted                  # Running simulators only
apple simulator list --available               # Available simulators only
apple simulator list --name "My iPhone 15"     # Filter by name
apple simulator list --device-type "iPhone 16 Pro"  # Filter by device type
apple simulator list --runtime "iOS 18.3"      # Filter by runtime
apple simulator list --product-family "iPhone" # Filter by product family
apple simulator list --format json             # JSON output
```

## Create Simulator

```bash
apple simulator create "My iPhone 15" --device-type "iPhone 15"
apple simulator create "My iPhone 15 Pro" --device-type "iPhone 15 Pro" --runtime "iOS 17.0"
apple simulator create "Apple Watch Series 9" --device-type "Apple Watch Series 9 (45mm)" --runtime "watchOS 10.0"
```

## Boot / Shutdown / Erase / Delete

```bash
# Boot - accepts UDID or name
apple simulator boot "My iPhone 15"
apple simulator boot --wait "My iPhone 15"              # Wait until ready
apple simulator boot --wait --timeout 180 <udid>        # Custom timeout

# Shutdown - accepts UDID, name, "booted", or "all"
apple simulator shutdown "My iPhone 15"
apple simulator shutdown booted                         # All running
apple simulator shutdown all                            # All simulators

# Erase (factory reset)
apple simulator erase "My iPhone 15"
apple simulator erase all

# Delete (permanent)
apple simulator delete "My Old iPhone"
apple simulator delete unavailable                      # Delete unavailable ones
```

## Screenshots

```bash
apple simulator screenshot booted                       # Save to current dir
apple simulator screenshot "My iPhone 15" --output ~/Desktop/screenshot.png
```

## App Management

```bash
apple simulator app install booted ~/MyApp.app
apple simulator app uninstall booted com.mycompany.myapp
apple simulator app launch booted com.mycompany.myapp
apple simulator app terminate booted com.mycompany.myapp
apple simulator app list booted                         # List installed apps
```

## Other Simulator Commands

```bash
apple simulator open                                    # Open Simulator.app
apple simulator open <udid>                             # Open specific simulator
apple simulator device-types                            # List available device types
apple simulator open-url booted "myapp://deeplink"      # Open URL/deep link
apple simulator logs booted                             # Stream logs
apple simulator logs booted --predicate "eventMessage contains 'error'"
```

---

# App Store Connect Commands

## Certificates

```bash
# List certificates
apple certificate list
apple certificate list --type IOS_DISTRIBUTION
apple certificate list --format json

# Create certificate (generates CSR automatically)
apple certificate create                                # Development cert
apple certificate create --type DISTRIBUTION --output ~/certs/
apple certificate create --type IOS_DISTRIBUTION --common-name "My Cert"

# Revoke certificate
apple certificate revoke <cert-id>
```

**Certificate Types**: `DEVELOPMENT`, `DISTRIBUTION`, `IOS_DEVELOPMENT`, `IOS_DISTRIBUTION`, `MAC_APP_DEVELOPMENT`, `MAC_APP_DISTRIBUTION`, `DEVELOPER_ID_APPLICATION`

## Bundle IDs

```bash
# List bundle IDs
apple bundleids list
apple bundleids list --platform IOS
apple bundleids list --identifier com.mycompany.*

# Create bundle ID
apple bundleids create "My App" com.mycompany.myapp IOS
apple bundleids create "Mac App" com.mycompany.macapp MAC_OS
apple bundleids create "Universal App" com.mycompany.app UNIVERSAL
apple bundleids create "Wildcard" "com.mycompany.*" IOS    # Wildcard ID

# Update / Delete
apple bundleids update <id> --name "New Name"
apple bundleids delete <id>
```

**Platforms**: `IOS`, `MAC_OS`, `UNIVERSAL`

## Registered Devices

```bash
# List devices
apple devices list
apple devices list --platform IOS
apple devices list --status ENABLED

# Register device
apple devices register "John's iPhone" 00008030-001234567890001E IOS
apple devices register "Test Mac" 12345678-1234-1234-1234-123456789012 MAC_OS

# Update device
apple devices update <id> --name "John's New iPhone"
apple devices update <id> --status DISABLED
```

## Provisioning Profiles

```bash
# List profiles from App Store Connect
apple provisioning list
apple provisioning list --type IOS_APP_DEVELOPMENT
apple provisioning list --bundle-id com.mycompany.myapp
apple provisioning list --download                      # Include profile data
apple provisioning list --install                       # Install to ~/Library/MobileDevice/

# List locally installed profiles
apple provisioning installed

# Parse a .mobileprovision file
apple provisioning parse MyApp.mobileprovision
apple provisioning parse ~/Downloads/profile.mobileprovision --format json

# Create profile
apple provisioning create \
  --name "My App Dev Profile" \
  --type IOS_APP_DEVELOPMENT \
  --bundle-id <bundle-id-resource-id> \
  --certificates <cert-id> \
  --devices <device-id1>,<device-id2>

apple provisioning create \
  --name "My App Store Profile" \
  --type IOS_APP_STORE \
  --bundle-id <bundle-id-resource-id> \
  --certificates <cert-id> \
  --download

# Delete profile
apple provisioning delete <profile-id>
```

**Profile Types**: `IOS_APP_DEVELOPMENT`, `IOS_APP_STORE`, `IOS_APP_ADHOC`, `IOS_APP_INHOUSE`, `MAC_APP_DEVELOPMENT`, `MAC_APP_STORE`, `MAC_APP_DIRECT`

---

# CI/CD Commands

## ci provision (Full CI Setup)

Sets up keychain, imports certificate, and downloads provisioning profiles in one command:

```bash
# Basic usage - certificate from env var, profiles from App Store Connect
apple ci provision \
  --certificate IOS_CERT_BASE64 \
  --bundle-identifier com.mycompany.myapp

# Full example with all options
apple ci provision \
  --certificate IOS_CERT_BASE64 \
  --certificate-passphrase "$CERT_PASSWORD" \
  --keychain ci-build.keychain \
  --keychain-password temp1234 \
  --bundle-identifier com.mycompany.myapp \
  --profile-type IOS_APP_STORE \
  --api-key-id "$APP_STORE_CONNECT_KEY_ID" \
  --api-issuer-id "$APP_STORE_CONNECT_ISSUER_ID" \
  --api-private-key "$APP_STORE_CONNECT_PRIVATE_KEY"
```

## ci deprovision (Cleanup)

```bash
apple ci deprovision --keychain ci-build.keychain
apple ci deprovision                                    # Uses default keychain name
```

## ci secret (Convert to Base64)

Convert certificates/keys to base64 for secure CI storage:

```bash
apple ci secret --from-certificate ios-distribution.p12
apple ci secret --from-private-key AuthKey_ABC123DEF4.p8
apple ci secret --from-certificate cert.p12 --output-file cert.b64
```

## ci base64-to-file / env-to-file

```bash
# Decode base64 from env var to file
apple ci base64-to-file --base64 IOS_CERT_BASE64 --output-file distribution.p12

# Write env var directly to file (no decoding)
apple ci env-to-file --env-var APP_STORE_CONNECT_API_KEY --output-file AuthKey.p8
```

---

# Keychain Commands

For manual keychain management on macOS:

```bash
# Create temporary keychain
apple keychain create --keychain ci-temp.keychain-db --password temp1234

# Unlock keychain
apple keychain unlock --keychain ci-temp.keychain-db --password temp1234
apple keychain unlock --allow-any-app-read --keychain ci-temp.keychain-db --password temp1234

# Import certificate
apple keychain import distribution.p12 --keychain ci-temp.keychain-db --passphrase certpass
apple keychain import dev.p12 --keychain ci-temp.keychain-db --allow-any-app-read

# Set default keychain
apple keychain default --keychain ci-temp.keychain-db

# Delete keychain
apple keychain delete --keychain ci-temp.keychain-db
```

---

# Xcode & Device Commands

```bash
# List installed Xcode versions
apple xcode list
apple xcode list --format json

# Find Xcode
apple xcode locate                                      # Currently selected
apple xcode locate --best                               # Best available version

# List connected physical devices
apple device list
apple device list --format json
```

---

# App Upload & Validation

```bash
# Validate app before upload
apple validate ~/MyApp.ipa --type ios
apple validate ~/MyApp.app --type macos

# Upload to App Store Connect / TestFlight
apple upload ~/MyApp.ipa --type ios
apple upload ~/MyApp.app --type macos

# With explicit credentials
apple upload ~/MyApp.ipa --type ios \
  --key-id ABC123DEF4 \
  --issuer-id 12345678-1234-1234-1234-123456789012 \
  --private-key ~/AuthKey.p8
```

**App Types**: `ios`, `macos`, `watchos`, `tvos`

---

# App Info

Read Info.plist metadata from an app bundle:

```bash
apple app-info ~/MyApp.app
apple app-info ~/MyApp.app --format json
apple app-info ~/MyApp.app --verbose
```

---

# Common Workflows

## CI Setup for iOS App

```bash
# 1. Store certificate as base64 in CI secrets
apple ci secret --from-certificate distribution.p12

# 2. In CI pipeline:
apple ci provision \
  --certificate "$IOS_CERT_BASE64" \
  --certificate-passphrase "$CERT_PASSWORD" \
  --bundle-identifier com.mycompany.myapp \
  --keychain ci-build.keychain

# 3. Build your app...
xcodebuild ...

# 4. Upload to TestFlight
apple upload output/MyApp.ipa --type ios

# 5. Cleanup
apple ci deprovision --keychain ci-build.keychain
```

## Create New App Setup

```bash
# 1. Create bundle ID
apple bundleids create "My New App" com.mycompany.newapp IOS

# 2. Get the bundle ID resource ID from output, then create profile
apple provisioning create \
  --name "My New App Development" \
  --type IOS_APP_DEVELOPMENT \
  --bundle-id <bundle-id-from-step-1> \
  --certificates <your-cert-id> \
  --all-devices \
  --download

# 3. Install the profile
apple provisioning list --bundle-id com.mycompany.newapp --install
```

## Test on Multiple Simulators

```bash
# Create simulators
apple simulator create "Test iPhone 15" --device-type "iPhone 15"
apple simulator create "Test iPhone SE" --device-type "iPhone SE (3rd generation)"

# Boot all
apple simulator boot "Test iPhone 15"
apple simulator boot "Test iPhone SE"

# Install app
apple simulator app install "Test iPhone 15" ~/MyApp.app
apple simulator app install "Test iPhone SE" ~/MyApp.app

# Take screenshots
apple simulator screenshot "Test iPhone 15" --output iphone15.png
apple simulator screenshot "Test iPhone SE" --output iphonese.png

# Cleanup
apple simulator delete "Test iPhone 15"
apple simulator delete "Test iPhone SE"
```
