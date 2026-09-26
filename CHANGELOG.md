# Changelog

All notable changes to Grim Search are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Older releases are also listed on the
[GitHub releases page](https://github.com/hallgeirl/grim-search/releases).

## [Unreleased]

### Fixed

- "Find duplicates" and "Find new items" now work for character names that
  contain spaces when using the Lucene search engine. The character name was
  indexed as analyzed text, so an exact query for a multi-word name (for
  example `The Peismaker`) never matched. Character names are now matched
  against an exact, case-insensitive keyword field.
- The character selection dropdown is no longer cleared when settings are
  saved or the index is otherwise rebuilt. The character list is now updated in
  place instead of being cleared, which preserves the current selection.

## [0.11.1] - 2026-09-21

### Changed

- The application version and runtime/OS details are logged at startup.

## [0.11.0] - 2026-09-21

### Added

- Resistance reduction modifiers are now included in searchable item stats.

### Changed

- Improved the Linux install scripts.

## [0.10.0] - 2026-09-17

### Added

- Item names can be displayed in other Grim Dawn languages installed with the
  game; items remain searchable by both their localized and English names.
- Bundled Noto Sans CJK JP font so Chinese, Japanese, and Korean names render
  consistently without system fonts.

## [0.9.4] - 2026-08-31

### Changed

- Log rotation keeps the last 10 archived logs with timestamped names; the
  active log is always `grimsearch.log`.

## [0.9.3] - 2026-08-30

### Fixed

- The character dropdown is now populated on startup when "Find duplicates" or
  "Find new items" mode is selected.

## [0.9.2] - 2026-08-27

### Added

- Display how long ago the index was last refreshed.

## [0.9.1] - 2026-08-23

### Fixed

- grimtools links now use `exact_match=0`, working around the broken exact-match
  search on grimtools.

## [0.9.0] - 2026-08-09

### Added

- GOG install detection, and Steam no longer needs to be running for detection.
- Option to include or exclude blueprints from search results.

### Fixed

- A race condition when loading data files.

## [0.8.1] - 2026-08-06

### Changed

- No user-facing changes recorded.

## [0.8.0] - 2026-08-05

### Added

- Hardcore character support.

## [0.7.0] - 2026-07-30

### Changed

- Toolchain updates, including Avalonia 12, NLog 6 and updated CI actions.

## [0.6.0-beta1] - 2026-07-24

### Changed

- Beta release with toolchain updates.

## [0.5.1] - 2026-07-24

### Fixed

- Compatibility with Grim Dawn 1.3.0 save files.

## [0.5.0] - 2026-07-24

### Changed

- No user-facing changes recorded.

## [0.4.5] - 2024-03-10

### Changed

- No user-facing changes recorded.

## [0.4.4] - 2024-03-10

### Changed

- No user-facing changes recorded.

## [0.4.2] - 2023-12-22

### Added

- Logging.

## [0.4.1] - 2023-12-17

### Added

- Infinite scrolling for search results.

## [0.4.0] - 2023-12-16

### Changed

- No user-facing changes recorded.

## [0.3.3] - 2023-11-20

### Fixed

- Character loading for Grim Dawn 1.2.

## [0.3.2] - 2023-11-19

### Added

- Damage numbers and pet damage modifiers are indexed and searchable.

## [0.3.1] - 2023-11-18

### Added

- Linux install script.

### Changed

- Configuration and logs are stored in the user's home directory instead of the
  application directory.

## [0.3.0] - 2023-11-18

### Added

- Linux support; moved to .NET 8 and removed the external ArchiveTool
  dependency.
- Automated release workflow.

[Unreleased]: https://github.com/hallgeirl/grim-search/compare/v0.11.1...HEAD
[0.11.1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.11.1
[0.11.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.11.0
[0.10.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.10.0
[0.9.4]: https://github.com/hallgeirl/grim-search/releases/tag/v0.9.4
[0.9.3]: https://github.com/hallgeirl/grim-search/releases/tag/v0.9.3
[0.9.2]: https://github.com/hallgeirl/grim-search/releases/tag/v0.9.2
[0.9.1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.9.1
[0.9.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.9.0
[0.8.1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.8.1
[0.8.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.8.0
[0.7.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.7.0
[0.6.0-beta1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.6.0-beta1
[0.5.1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.5.1
[0.5.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.5.0
[0.4.5]: https://github.com/hallgeirl/grim-search/releases/tag/v0.4.5
[0.4.4]: https://github.com/hallgeirl/grim-search/releases/tag/v0.4.4
[0.4.2]: https://github.com/hallgeirl/grim-search/releases/tag/v0.4.2
[0.4.1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.4.1
[0.4.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.4.0
[0.3.3]: https://github.com/hallgeirl/grim-search/releases/tag/v0.3.3
[0.3.2]: https://github.com/hallgeirl/grim-search/releases/tag/v0.3.2
[0.3.1]: https://github.com/hallgeirl/grim-search/releases/tag/v0.3.1
[0.3.0]: https://github.com/hallgeirl/grim-search/releases/tag/v0.3.0
