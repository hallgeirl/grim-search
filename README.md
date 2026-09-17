# Grim Search

Someone that has played Grim Dawn for a while may start losing track of all his or her items across the various characters, mules and stashes. This application aims to resolve this by allowing you to search for items, based on item names, certain stats, as well as filtering based on quality, level requirements and item types. Additionally, you can easily find out what items on a particular character is also found on others.

There is also a thread on the Grim Dawn forums: https://forums.crateentertainment.com/t/tool-grim-search/45671

## Installation

### Linux

The Linux install script downloads the latest Grim Search release, installs it to `/usr/share/grimsearch`, and adds an application launcher. Run it from the repository root:

```sh
cd installscripts
sudo ./install-linux.sh
```

The script requires `curl`, `jq`, `wget`, `unzip`, and `desktop-file-install`. See the [install scripts documentation](installscripts/README.md) for additional details and caveats.

### Windows

There is currently no automated Windows installer. Download the Windows archive from the [latest release](https://github.com/hallgeirl/grim-search/releases/latest), extract it, and run `GrimSearch.exe`.

## First time setup
Go to the Settings tab to configure your save path and path to Grim Dawn. Click "Detect Grim Dawn settings" to detect standard Steam or GOG locations automatically. Steam does not need to be running. If detection fails, you can enter the paths manually.

The Settings tab also lets you select an item language from the localization archives installed with Grim Dawn. Localized item names are displayed in the selected language and remain searchable by both their localized and English names.

Grim Search bundles Noto Sans CJK JP under the SIL Open Font License so Chinese, Japanese, and Korean item names render consistently without requiring system fonts.

## Searching for items
Simply type the item name, or parts of it, in the search box. You can also search by character name or certain stats. Filters are available as well.
![Regular search](https://github.com/hallgeirl/gd-item-search/blob/master/assets/GDItemSearchRegularSearch.png)
![Regular search on stats](https://github.com/hallgeirl/gd-item-search/blob/master/assets/GDItemSearchQualityFilter.png)

## Search for duplicate items across characters
From the dropdown, choose "Duplicate search" instead of "Regular search" and type in the full character name.
![Regular search](https://github.com/hallgeirl/gd-item-search/blob/master/assets/GDItemSearchDuplicateSearch.png)

## Contributing
If you want to contribute, please fork my repository and submit a pull request. 

## Acknowledgements
Thanks to marius00 @ github (https://github.com/marius00) for providing sample code for how to read blueprints.

## Community
There's not that big of a community yet, but there IS a thread on the Grim Dawn forums. 
