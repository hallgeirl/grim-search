#!/bin/bash
set -e

mkdir -p /usr/share/grimsearch

unzip $1 -d /usr/share/grimsearch
cp grimsearch.png /usr/share/pixmaps
desktop-file-install --dir=/usr/share/applications grimsearch.desktop
update-desktop-database -v /usr/share/applications