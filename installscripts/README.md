# Install scripts

Installs Grim Search and its launcher/icon.

## Installing Grim Search on Linux

Install the latest release without cloning the repository:

```sh
curl -fsSL https://github.com/hallgeirl/grim-search/releases/latest/download/install-linux.sh | sudo bash
```

`install-linux.sh`, `grimsearch.png`, and `grimsearch.desktop` are published as
standalone assets on each release, so the script can be piped straight to a
shell. From a checkout, run it from any directory instead — it always installs
the latest release:

```sh
sudo ./install-linux.sh
```

The script always installs the latest Linux (x64) release:

- Downloads the application, its icon, and its launcher to a temporary directory.
- Installs Grim Search to `/usr/share/grimsearch` and the launcher and icon for
  your desktop environment.
- Refreshes the desktop database so the app appears in your application menu.
- Removes the temporary files when it finishes.

### Dependencies

`curl`, `jq`, and `unzip` are required.

`desktop-file-install` and `update-desktop-database` from `desktop-file-utils`
are optional. When missing, the launcher is still installed by copying the
`.desktop` file.

Install them with one of:

```sh
# Debian/Ubuntu
sudo apt install curl unzip jq desktop-file-utils

# Fedora
sudo dnf install curl unzip jq desktop-file-utils
```

The script must be run as root; it will tell you if it is not.

## Installing Grim Search on Windows

There is no automated Windows installer yet. Download the Windows archive from
the [latest release](https://github.com/hallgeirl/grim-search/releases/latest),
extract it, and run `GrimSearch.exe`.
