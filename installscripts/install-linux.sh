#!/usr/bin/env bash
set -euo pipefail

APP_NAME="grimsearch"
INSTALL_DIR="/usr/share/grimsearch"
ICON_NAME="grimsearch.png"
DESKTOP_NAME="grimsearch.desktop"
API_URL="https://api.github.com/repos/hallgeirl/grim-search/releases/latest"
LATEST_BASE="https://github.com/hallgeirl/grim-search/releases/latest/download"

# `$0` is "bash" when the script is piped; only used to tailor the root hint.
SCRIPT_SOURCE="${BASH_SOURCE[0]:-}"

# Everything is downloaded into a private temp directory and removed on exit.
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

log() { printf '%s\n' "$*"; }
err() { printf 'Error: %s\n' "$*" >&2; }

require() {
    local cmd missing=()
    for cmd in "$@"; do
        command -v "$cmd" >/dev/null 2>&1 || missing+=("$cmd")
    done
    if ((${#missing[@]})); then
        err "Missing required tools: ${missing[*]}"
        log "Install them first, e.g.:"
        log "  Debian/Ubuntu: sudo apt install curl jq unzip desktop-file-utils"
        log "  Fedora:        sudo dnf install curl jq unzip desktop-file-utils"
        exit 1
    fi
}

download() {
    local url="$1" dest="$2" what="$3"
    if ! curl -fsSL "$url" -o "$dest"; then
        err "Could not download $what from $url"
        exit 1
    fi
}

# Require root before touching the filesystem.
if [[ "${EUID:-$(id -u)}" -ne 0 ]]; then
    if [[ -z "$SCRIPT_SOURCE" ]]; then
        err "This installer must be run as root. Try:"
        err "  curl -fsSL $LATEST_BASE/install-linux.sh | sudo bash"
    else
        err "This installer must be run as root. Try: sudo $0"
    fi
    exit 1
fi

# Only Linux x64 builds are published.
MACHINE="$(uname -m)"
if [[ "$MACHINE" != "x86_64" && "$MACHINE" != "amd64" ]]; then
    err "Only Linux x64 builds are published; detected $MACHINE."
    exit 1
fi

require curl jq unzip

log "Finding latest Linux release..."
if ! RELEASE_JSON="$(curl -fsSL "$API_URL")"; then
    err "Could not reach GitHub. Check your network connection and try again."
    exit 1
fi

ASSET_URL="$(printf '%s' "$RELEASE_JSON" \
    | jq -r \
        '.assets[]
         | select(.name | startswith("grimsearch-linux-x64-"))
         | select(.name | endswith(".zip"))
         | .browser_download_url' \
    | head -n 1)"

if [[ -z "$ASSET_URL" ]]; then
    err "Could not find the Linux x64 release asset."
    log "See https://github.com/hallgeirl/grim-search/releases"
    exit 1
fi

log "Downloading $ASSET_URL"
download "$ASSET_URL" "$TMP_DIR/$APP_NAME.zip" "the application archive"
mkdir -p "$TMP_DIR/unpacked"
unzip -oq "$TMP_DIR/$APP_NAME.zip" -d "$TMP_DIR/unpacked"

log "Downloading launcher and icon..."
download "$LATEST_BASE/$ICON_NAME" "$TMP_DIR/$ICON_NAME" "the application icon"
download "$LATEST_BASE/$DESKTOP_NAME" "$TMP_DIR/$DESKTOP_NAME" "the desktop launcher"

log "Installing to $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"
cp -a "$TMP_DIR/unpacked/." "$INSTALL_DIR/"

install -Dm644 "$TMP_DIR/$ICON_NAME" "/usr/share/pixmaps/$ICON_NAME"

if command -v desktop-file-install >/dev/null 2>&1; then
    desktop-file-install --dir=/usr/share/applications "$TMP_DIR/$DESKTOP_NAME"
else
    install -Dm644 "$TMP_DIR/$DESKTOP_NAME" "/usr/share/applications/$DESKTOP_NAME"
fi

if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database /usr/share/applications || true
fi

log "Grim Search installed. Launch it from your application menu."
