#!/bin/bash
set -e

# Script to update idb_companion binary from Facebook's IDB GitHub releases
# Usage: ./scripts/update-idb-companion.sh [version]
# 
# SECURITY NOTICE:
# - For production use, always specify a version explicitly (e.g., ./update-idb-companion.sh v1.2.3)
# - Avoid using "latest" to prevent supply-chain attacks if the upstream release is compromised
# - The script will verify checksums when available to ensure binary integrity
# - Pin specific versions in CI/CD pipelines for reproducible builds
#
# If no version specified, fetches the latest release (not recommended for production)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
IDB_PROJECT_DIR="$PROJECT_DIR/AppleDev.FbIdb"
RUNTIMES_DIR="$IDB_PROJECT_DIR/runtimes/osx-arm64/native"
VERSION_FILE="$IDB_PROJECT_DIR/idb-companion-version.json"

echo "🔍 Fetching release information from GitHub..."

if [ -n "$1" ]; then
    VERSION="$1"
    RELEASE_URL="https://api.github.com/repos/facebook/idb/releases/tags/$VERSION"
else
    RELEASE_URL="https://api.github.com/repos/facebook/idb/releases/latest"
fi

# Get release info
RELEASE_INFO=$(curl -sL "$RELEASE_URL")

# Extract version tag
VERSION=$(echo "$RELEASE_INFO" | grep -o '"tag_name": "[^"]*' | head -1 | cut -d'"' -f4)
COMMIT=$(echo "$RELEASE_INFO" | grep -o '"target_commitish": "[^"]*' | head -1 | cut -d'"' -f4)

if [ -z "$VERSION" ]; then
    echo "❌ Failed to get release information"
    exit 1
fi

echo "📦 Found version: $VERSION"

# Find the download URL for the companion tarball
DOWNLOAD_URL=$(echo "$RELEASE_INFO" | grep -o '"browser_download_url": "[^"]*idb-companion[^"]*\.tar\.gz"' | head -1 | cut -d'"' -f4)

if [ -z "$DOWNLOAD_URL" ]; then
    echo "❌ No idb-companion tarball found in release assets"
    echo "Available assets:"
    echo "$RELEASE_INFO" | grep -o '"browser_download_url": "[^"]*"' | cut -d'"' -f4
    exit 1
fi

echo "📥 Downloading from: $DOWNLOAD_URL"

# Create temp directory
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

# Download the tarball
curl -sL "$DOWNLOAD_URL" -o "$TEMP_DIR/idb-companion.tar.gz"

# Verify checksum if available
CHECKSUM_URL=$(echo "$RELEASE_INFO" | grep -o '"browser_download_url": "[^"]*checksums\.txt"' | head -1 | cut -d'"' -f4)
if [ -n "$CHECKSUM_URL" ]; then
	echo "🔐 Verifying checksum..."
	curl -sL "$CHECKSUM_URL" -o "$TEMP_DIR/checksums.txt"
	
	# Extract checksum for the companion tarball
	TARBALL_NAME=$(basename "$DOWNLOAD_URL")
	EXPECTED_CHECKSUM=$(grep "$TARBALL_NAME" "$TEMP_DIR/checksums.txt" | awk '{print $1}')
	
	if [ -n "$EXPECTED_CHECKSUM" ]; then
		ACTUAL_CHECKSUM=$(shasum -a 256 "$TEMP_DIR/idb-companion.tar.gz" | awk '{print $1}')
		
		if [ "$EXPECTED_CHECKSUM" != "$ACTUAL_CHECKSUM" ]; then
			echo "❌ Checksum verification failed!"
			echo "   Expected: $EXPECTED_CHECKSUM"
			echo "   Actual:   $ACTUAL_CHECKSUM"
			exit 1
		fi
		echo "✅ Checksum verified successfully"
	else
		echo "⚠️  Warning: Checksum not found in checksums.txt for $TARBALL_NAME"
	fi
else
	echo "⚠️  Warning: No checksums.txt file available for release $VERSION"
	echo "   Proceeding without checksum verification (not recommended for production)"
fi

echo "📂 Extracting..."

# Extract
tar -xzf "$TEMP_DIR/idb-companion.tar.gz" -C "$TEMP_DIR"

# Find the idb_companion binary
IDB_BINARY=$(find "$TEMP_DIR" -name "idb_companion" -type f | head -1)

if [ -z "$IDB_BINARY" ]; then
    echo "❌ idb_companion binary not found in archive"
    ls -la "$TEMP_DIR"
    exit 1
fi

# Create runtimes directory
mkdir -p "$RUNTIMES_DIR"

# Copy binary
cp "$IDB_BINARY" "$RUNTIMES_DIR/idb_companion"
chmod +x "$RUNTIMES_DIR/idb_companion"

echo "✅ Binary installed to: $RUNTIMES_DIR/idb_companion"

# Update version file
cat > "$VERSION_FILE" << EOF
{
  "version": "$VERSION",
  "commit": "$COMMIT",
  "updated": "$(date +%Y-%m-%d)",
  "source": "https://github.com/facebook/idb/releases"
}
EOF

echo "📝 Updated version file: $VERSION_FILE"

# Show version info
echo ""
echo "🎉 Successfully updated idb_companion to $VERSION"
cat "$VERSION_FILE"
