# E2E Test App

Minimal .NET MAUI app used by the CI workflow (`.github/workflows/run.yml`) to validate simulator operations end-to-end.

On startup the app logs:
- `E2E_APP_STARTED_SUCCESSFULLY` — proves the app launched
- `E2E_ENV_RECEIVED:<value>` — proves environment variables were forwarded via `SIMCTL_CHILD_*`

## Rebuilding the zip

The CI workflow uses a pre-built `.app` bundle from `AppleDev.Test/testdata/com.companyname.e2etestapp.zip`. If you change this app, rebuild and update the zip:

```bash
# From the repo root, build for iOS Simulator:
dotnet build AppleDev.Test.E2ETestApp/E2ETestApp.csproj \
  -f net10.0-ios \
  -r iossimulator-arm64

# If your Xcode version doesn't exactly match the .NET iOS SDK requirement,
# add -p:ValidateXcodeVersion=false to skip the version check.

# Zip the .app bundle:
cd AppleDev.Test.E2ETestApp/bin/Debug/net10.0-ios/iossimulator-arm64
rm -f ../../../../../AppleDev.Test/testdata/com.companyname.e2etestapp.zip
zip -r ../../../../../AppleDev.Test/testdata/com.companyname.e2etestapp.zip E2ETestApp.app
```

Then commit the updated zip.
