# AppleDev.Test

This directory contains tests for the AppleDev.Tools project.

## Running App Store Connect Tests

The App Store Connect API tests require valid credentials to run. You can provide these credentials using environment variables or a `.env` file.

### Using a .env file (Recommended)

1. Copy the `.env.example` file from the project root to `.env`:
   ```bash
   cp .env.example .env
   ```

2. Edit the `.env` file and fill in your App Store Connect API credentials:
   ```
   APP_STORE_CONNECT_KEY_ID=your_key_id_here
   APP_STORE_CONNECT_ISSUER_ID=your_issuer_id_here
   APP_STORE_CONNECT_PRIVATE_KEY=your_base64_encoded_private_key_here
   ```

3. Run the tests:
   ```bash
   dotnet test
   ```

The `.env` file is automatically loaded when tests run and is ignored by git for security.

### Using Environment Variables

Alternatively, you can set the environment variables directly:

```bash
export APP_STORE_CONNECT_KEY_ID=your_key_id_here
export APP_STORE_CONNECT_ISSUER_ID=your_issuer_id_here
export APP_STORE_CONNECT_PRIVATE_KEY=your_base64_encoded_private_key_here
dotnet test
```

### Getting App Store Connect API Credentials

1. Log in to [App Store Connect](https://appstoreconnect.apple.com/)
2. Go to Users and Access > Keys
3. Create a new API Key or use an existing one
4. Download the private key (.p8 file)
5. Convert the private key to base64:
   ```bash
   base64 -i AuthKey_XXXXXXXXXX.p8 | tr -d '\n'
   ```
6. Use the Key ID and Issuer ID shown in App Store Connect

### Skipped Tests

If credentials are not configured, App Store Connect tests will be automatically skipped with the message:
```
App Store Connect credentials not configured
```

