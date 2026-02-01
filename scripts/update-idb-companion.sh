#!/bin/bash
set -e

# Script to update idb_companion binary from Facebook's IDB GitHub releases
# Usage: ./scripts/update-idb-companion.sh [version]
# If no version specified, fetches the latest release

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
