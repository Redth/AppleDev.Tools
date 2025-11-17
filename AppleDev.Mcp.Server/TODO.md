# MCP Server Implementation TODO

This document tracks which CLI commands from AppleDev.Tool have been implemented as MCP tools and which are still pending.

## Implementation Status

### ✅ Implemented (15 tools)

#### App Store Connect - Devices (3/3)
- [x] ListDevices
- [x] RegisterDevice
- [x] ModifyDevice (UpdateDevice in CLI)

#### App Store Connect - Bundle IDs (4/4)
- [x] ListBundleIds
- [x] CreateBundleId
- [x] UpdateBundleId
- [x] DeleteBundleId

#### App Store Connect - Certificates (3/3)
- [x] ListCertificates
- [x] CreateCertificate
- [x] RevokeCertificate

#### App Store Connect - Provisioning Profiles (5/5)
- [x] ListProvisioningProfiles
- [x] CreateProvisioningProfile
- [x] DeleteProvisioningProfile
- [x] ListInstalledProvisioningProfiles
- [x] ParseProvisioningProfile

---

## 📋 Pending Implementation (34 tools)

### App (3 tools)
- [ ] **AppInfo** - Get information about an .app bundle
  - Input: Path to .app or .ipa file
  - Output: Bundle identifier, version, build number, signing info, etc.

- [ ] **UploadApp** - Upload an app to App Store Connect
  - Input: .ipa path, credentials
  - Output: Upload status, build processing info

- [ ] **ValidateApp** - Validate an app with App Store Connect
  - Input: .ipa path, credentials
  - Output: Validation results, warnings, errors

### CI/Provisioning (5 tools)
- [ ] **ProvisionCi** - Provision a CI environment with certificates and profiles
  - Input: Bundle IDs, certificate types, device UDIDs
  - Output: Created/downloaded certificates and profiles

- [ ] **DeprovisionCi** - Clean up CI provisioning artifacts
  - Input: Optional filter criteria
  - Output: Cleanup status

- [ ] **CreateSecret** - Create a certificate signing request and private key
  - Input: Common name, output paths
  - Output: CSR and private key files

- [ ] **Base64ToFile** - Decode base64 string to file
  - Input: Base64 string, output path
  - Output: Decoded file

- [ ] **EnvironmentVariableToFile** - Write environment variable to file
  - Input: Env var name, output path
  - Output: File with env var contents

### Physical Devices (1 tool)
- [ ] **ListDevices** (xcdevice) - List connected physical devices
  - Input: Optional filters
  - Output: Connected device info (name, UDID, OS version, model)

### Keychain (5 tools)
- [ ] **CreateKeychain** - Create a new keychain
  - Input: Keychain name, password
  - Output: Created keychain path

- [ ] **DeleteKeychain** - Delete a keychain
  - Input: Keychain name/path
  - Output: Deletion status

- [ ] **UnlockKeychain** - Unlock a keychain
  - Input: Keychain name/path, password
  - Output: Unlock status

- [ ] **SetDefaultKeychain** - Set the default keychain
  - Input: Keychain name/path
  - Output: Status

- [ ] **ImportPkcsKeychain** - Import PKCS12 certificate into keychain
  - Input: .p12 file path, password, keychain name
  - Output: Import status

### Simulators (15 tools)
- [ ] **ListSimulators** - List available simulators
  - Input: Optional filters (runtime, device type, state)
  - Output: Simulator list with UDID, name, state, runtime

- [ ] **CreateSimulator** - Create a new simulator
  - Input: Name, device type, runtime
  - Output: Created simulator UDID

- [ ] **DeleteSimulator** - Delete a simulator
  - Input: Simulator UDID or name
  - Output: Deletion status

- [ ] **BootSimulator** - Boot a simulator
  - Input: Simulator UDID
  - Output: Boot status

- [ ] **ShutdownSimulator** - Shutdown a simulator
  - Input: Simulator UDID
  - Output: Shutdown status

- [ ] **EraseSimulator** - Erase all content from a simulator
  - Input: Simulator UDID
  - Output: Erase status

- [ ] **OpenSimulator** - Open Simulator.app
  - Input: Optional simulator UDID to open
  - Output: Status

- [ ] **DeviceTypesSimulator** - List available device types
  - Input: Optional runtime filter
  - Output: Device type list

- [ ] **ListSimulatorApps** - List installed apps on a simulator
  - Input: Simulator UDID
  - Output: Installed app list (bundle ID, name, path)

- [ ] **InstallSimulatorApp** - Install an app on a simulator
  - Input: Simulator UDID, .app path
  - Output: Installation status

- [ ] **UninstallSimulatorApp** - Uninstall an app from a simulator
  - Input: Simulator UDID, bundle identifier
  - Output: Uninstall status

- [ ] **LaunchSimulatorApp** - Launch an app on a simulator
  - Input: Simulator UDID, bundle identifier, optional arguments
  - Output: Launch status, process ID

- [ ] **TerminateSimulatorApp** - Terminate an app on a simulator
  - Input: Simulator UDID, bundle identifier
  - Output: Termination status

- [ ] **OpenUrlSimulator** - Open a URL in a simulator
  - Input: Simulator UDID, URL
  - Output: Status

- [ ] **ScreenshotSimulator** - Take a screenshot of a simulator
  - Input: Simulator UDID, output path
  - Output: Screenshot file path

- [ ] **LogsSimulator** - Stream or retrieve simulator logs
  - Input: Simulator UDID, optional filters
  - Output: Log stream or log file

### Xcode (2 tools)
- [ ] **ListXcode** - List installed Xcode versions
  - Input: None
  - Output: Xcode installations (path, version, build number)

- [ ] **LocateXcode** - Find the active Xcode installation
  - Input: None
  - Output: Active Xcode path and version



---

## Priority Recommendations

### High Priority
1. **Simulator Management** - Core development workflow tools
   - ListSimulators, CreateSimulator, BootSimulator, ShutdownSimulator, EraseSimulator

2. **App Installation & Testing** - Essential for automated testing
   - InstallSimulatorApp, LaunchSimulatorApp, ListSimulatorApps

3. **Physical Devices** - Important for real device testing
   - ListDevices (xcdevice)

### Medium Priority
4. **Keychain Management** - CI/CD integration
   - CreateKeychain, UnlockKeychain, ImportPkcsKeychain

5. **App Information** - Useful for validation workflows
   - AppInfo

6. **Local Profile Management** - Development convenience
   - ListInstalledProvisioningProfiles, ParseProvisioningProfiles

### Lower Priority
7. **CI Provisioning** - Can be composed from other tools
   - ProvisionCi, DeprovisionCi

8. **Xcode Management** - Less frequently needed
   - ListXcode, LocateXcode

9. **Utility Tools** - Nice to have
   - Base64ToFile, EnvironmentVariableToFile, CreateSecret

---

## Implementation Notes

### Considerations for MCP Tools

1. **Async Operations**: Many simulator operations (boot, install, launch) may take time. Consider returning status updates or implementing polling mechanisms.

2. **File Handling**: Tools like ScreenshotSimulator, AppInfo return or require file paths. MCP tools should handle file I/O appropriately.

3. **Streaming Output**: LogsSimulator would benefit from streaming capabilities if MCP supports it.

4. **Error Handling**: Physical device operations may fail if devices are disconnected. Ensure graceful error messages.

5. **Platform Dependencies**: All these tools require macOS. Document platform requirements clearly.

6. **Authorization**: Some operations (keychain, simulators) may require additional macOS permissions.

---

## Current Statistics

- **Total CLI Commands**: 49
- **Implemented in MCP**: 15 (30.6%)
- **Pending Implementation**: 34 (69.4%)

### By Category
- App Store Connect: 15/15 (100%) ✅
- Simulators: 0/15 (0%)
- Keychain: 0/5 (0%)
- CI/Provisioning: 0/5 (0%)
- App Operations: 0/3 (0%)
- Physical Devices: 0/1 (0%)
- Xcode: 0/2 (0%)
