# MCP Server Implementation TODO

This document tracks which CLI commands from AppleDev.Tool have been implemented as MCP tools and which are still pending.

## Implementation Status

### ✅ Implemented (36 tools)

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

#### App (1/1)
- [x] GetAppInfo

#### Simulators (15/15)
- [x] ListSimulators
- [x] CreateSimulator
- [x] DeleteSimulator
- [x] BootSimulator
- [x] ShutdownSimulator
- [x] EraseSimulator
- [x] OpenSimulator
- [x] ListSimulatorDeviceTypes
- [x] ListSimulatorApps
- [x] InstallSimulatorApp
- [x] UninstallSimulatorApp
- [x] LaunchSimulatorApp
- [x] TerminateSimulatorApp
- [x] OpenUrlSimulator
- [x] ScreenshotSimulator
- [x] GetSimulatorLogs

---

## 📋 Pending Implementation (8 tools)

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
- **Implemented in MCP**: 36 (81.8%)
- **Pending Implementation**: 8 (18.2%)

### By Category
- App Store Connect: 17/17 (100%) ✅
- Physical Devices: 1/1 (100%) ✅
- Xcode: 2/2 (100%) ✅
- App Operations: 1/1 (100%) ✅
- Simulators: 15/15 (100%) ✅
- Keychain: 0/5 (0%)

