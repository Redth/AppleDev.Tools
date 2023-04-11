# AppleDev.Tools

.NET Library with useful Apple/Xcode tool wrappers and implementations for developers including a global .NET CLI tool

![image](https://user-images.githubusercontent.com/271950/231289451-0db771e3-c2f6-4b85-a3ea-e80c70439d48.png)

## CLI Tool Features
- Simulators: List, boot (and wait), shutdown, erase, screenshot
- Devices: List
- Keychain: Import cert
- Provisioning Profiles: List, Download/Install
- Certificates: Create
- More planned!

## Library API's

### Xcode
- Locate

### XCRun
- Locate
- Install AuthKey_*.p8 files

### XCDevice
- List devices / simulators
- Observe device changes

### ALTool
- Upload app to app store / test flight
- Validate app

### SimCtl
- List Simulators
- Boot simulator (optionally wait for ready)
- Delete simulator
- Erase Simulator
- Open Simulator.app
- Add media to simulator
- Install/Uninstall apps in simulator
- Launch/Terminate apps in simulator
- Open URL in simulator
- Screenshot of simulator

### AppStoreConnect
- List, Download/Install, Create Provisioning Profiles
- List, Create, Revoke Certificates
- List, Create Bundle ID's
- Register Devices
