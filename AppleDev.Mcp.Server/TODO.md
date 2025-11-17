# MCP Server Implementation TODO

This document tracks which CLI commands from AppleDev.Tool have been implemented as MCP tools and which are still pending.

## Implementation Status

### ✅ Implemented (20 tools)

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

#### App Store Connect - Provisioning Profiles (7/7)
- [x] ListProvisioningProfiles
- [x] CreateProvisioningProfile
- [x] DeleteProvisioningProfile
- [x] ListInstalledProvisioningProfiles
- [x] ParseProvisioningProfile
- [x] DownloadProvisioningProfile
- [x] InstallProvisioningProfile

#### Physical Devices (1/1)
- [x] ListDevicesAndSimulators

#### Xcode (2/2)
- [x] ListXcode
- [x] LocateXcode

---

## 📋 Pending Implementation (24 tools)

### App (1 tool)
- [ ] **AppInfo** - Get information about an .app bundle
  - Input: Path to .app or .ipa file
  - Output: Bundle identifier, version, build number, signing info, etc.

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

- **Total Tools**: 44
- **Implemented in MCP**: 20 (45.5%)
- **Pending Implementation**: 24 (54.5%)

### By Category
- App Store Connect: 17/17 (100%) ✅
- Physical Devices: 1/1 (100%) ✅
- Xcode: 2/2 (100%) ✅
- Simulators: 0/15 (0%)
- Keychain: 0/5 (0%)
- App Operations: 0/1 (0%)

