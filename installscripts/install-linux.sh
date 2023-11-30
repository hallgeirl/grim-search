#!/bin/bash
set -e

echo "Finding latest release..."
export LATEST_RELEASE_URL=$(curl -s https://api.github.com/repos/hallgeirl/grim-search/releases/latest | jq -r ".assets[] | select(.name | test(\"grimsearch-linux\")) | .browser_download_url")

echo "Downloading $LATEST_RELEASE_URL"
wget $LATEST_RELEASE_URL -O /tmp/grimsearch.zip

mkdir -p /usr/share/grimsearch

echo "Unzipping"
unzip -o /tmp/grimsearch.zip -d /usr/share/grimsearch
cp grimsearch.png /usr/share/pixmaps
desktop-file-install --dir=/usr/share/applications grimsearch.desktop
update-desktop-database -v /usr/share/applications

echo "Removing archive /tmp/grimsearch.zip"
rm /tmp/grimsearch.zip