# AGENTS.md

Guidance for coding agents working in the Grim Search repository.

## What this is

Grim Search is a cross-platform (Windows/Linux) desktop app for Action RPG *Grim Dawn*.
It reads a player's save files and the game's item databases, then lets the user search
items by name, stats, quality, level and type, find duplicates across characters, and find
items unique to one character. UI is Avalonia; item indexing is in a separate library.

License: GPLv3. Default branch: `master` (do not commit directly to it; use PRs).

## Tech stack

- **.NET 10** (`net10.0`) — the .NET 10 SDK is required (`dotnet --version` shows 10.x here).
- **Avalonia 12** UI + **ReactiveUI.Avalonia** (MVVM), compiled bindings enabled.
- **Lucene.Net 4.8.0-beta** for the optional "Lucene" search engine.
- **Newtonsoft.Json** for serialization/caches, **NLog 6** for logging, **K4os.Compression.LZ4** for archive decompression.
- **MSTest** (Microsoft.NET.Test.Sdk + MSTest.TestAdapter/TestFramework) for tests.
- **GitHub Actions** builds/tests and publishes releases; **Renovate** updates dependencies.

## Repository layout

```
GrimSearch/            Avalonia desktop app (exe)
  ViewModels/          MainViewModel (all app logic), ItemViewModel, helpers
  Views/               MainWindow.axaml(.cs), MessageBox
  Controls/            MultiselectComboControl
  Common/              small helpers / event args
  Assets/              icon + bundled Noto Sans CJK JP font (SIL OFL)
  Settings.cs          StoredSettings DTO persisted as JSON
  Program.cs / App.axaml.cs   entry point + Avalonia/ReactiveUI/NLog init
  nlog.config          console + rolling file logger (logs to config folder)
GrimSearch.Utils/      Core logic library (no UI), unsafe blocks enabled
  CharacterFiles/      Save-file parsers (player.gdc, .gst/.gsh, .bst, etc.)
  DBFiles/             Grim Dawn DB readers (ArzExtractor, ItemCache, StringsCache, ItemHelper)
  Steam/               Steam/GOG folder auto-detection (VDF parsing, registry on Windows)
  IIndex/IndexBase     Search abstraction + shared index build pipeline
  Index.cs             "Classic" in-memory LINQ search engine
  LuceneIndex.cs       RAM-directory Lucene search engine
  IndexFilter/IndexItem/SearchResult/IndexSummary   query + result models
  MD5Store, Metrics, ConfigFileHelper               hash cache, metrics, config paths
GrimSearch.Tests/      MSTest project; fixtures under Resources/
installscripts/        Linux installer + .desktop/.png; README documents it
.github/workflows/build.yaml   CI: restore, build, test, publish, release
```

## Build / test / run

Run from the repository root:

```sh
dotnet restore
dotnet build --no-restore           # or: dotnet build GrimSearch.sln
dotnet test                          # runs GrimSearch.Tests (MSTest)
dotnet run --project GrimSearch      # launch the desktop app
```

Publish self-contained, single-file builds (as CI does):

```sh
dotnet publish GrimSearch/GrimSearch.csproj --self-contained -r linux-x64
dotnet publish GrimSearch/GrimSearch.csproj --self-contained -r win-x64
```

There is no `GitVersion.yml`; CI derives the version with GitVersion 6 defaults and passes
`-p:Version` / `-p:InformationalVersion` into publish. Tags matching `*` create a GitHub release.

### Tests

- Tests live in `GrimSearch.Tests/` and use MSTest (`[TestClass]` / `[TestMethod]`).
- They build indexes from checked-in fixtures in `GrimSearch.Tests/Resources/`
  (`ItemsCache.json`, `TagsCache.json`, and a tree of save files under `Resources/Saves`).
- Tests set the singleton caches explicitly, e.g.
  `StringsCache.Instance.CacheFilename = "Resources/TagsCache.json"` and
  `ItemCache.Instance.CacheFilename = "Resources/ItemsCache.json"`. Follow that pattern when
  adding tests, and set `IsDirty = true` to force a reload.
- `GrimSearch.Utils` exposes internals to the test project via `InternalsVisibleTo("GrimSearch.Tests")`.
- When adding a new fixture file, add it to `GrimSearch.Tests/GrimSearch.Tests.csproj` with
  `CopyToOutputDirectory` so tests can find it.

## Architecture

### Startup flow
`Program.Main` → `App.Initialize` (NLog setup, log dir = config folder, version log) →
`App.OnFrameworkInitializationCompleted` creates `MainWindow` with a `MainViewModel`.
`MainViewModel.Initialize` → `LoadSettingsAsync` → `BuildIndexAsync` → `SearchAsync`.

### Configuration & caches
`ConfigFileHelper.GetConfigFolder()` returns `%LocalAppData%/grimsearch`
(on Linux `~/.local/share/grimsearch`). Files there:

- `GDItemSearchSettings.json` — persisted `StoredSettings` (paths, language, filters, engine, etc.).
- `ItemsCache.json` — parsed game item DB; cleared when the game DB changes (version stamp + date).
- `TagsCache.json` — localized + English string tables.
- `DatabaseHashes.json` — MD5s of processed archives (`MD5Store`).
- `grimsearch.log` — NLog rolling file.

`ItemCache` and `StringsCache` are singletons with an `IsDirty` flag; saving settings marks them
dirty so the next index build reloads them. `ClearCacheCommand` deletes the caches.

### Index pipeline
`IIndex` (`BuildAsync`, `FindAsync`, `FindDuplicatesAsync`, `FindUniqueAsync`) is implemented by
`IndexBase`, which owns the shared `Build` flow under a static lock:

1. `CharacterLoader.LoadAllCharacters(savesDir, ...)` parses `main/<char>/player.gdc`, the transfer
   stash (`transfer.gst`/`.gsh` + `reagents.*`), and blueprints (`formulas.gst`; a custom
   `formulasFilename` can be passed for external recipes). Backup dirs starting with `__` are skipped.
2. `StringsCache.LoadAllStrings(grimDawnDir)` reads `Text_<lang>.arc` (English always, plus the
   selected language). Localized and English names are both indexed/searchable.
3. `ItemCache.LoadAllItems(grimDawnDir, ...)` extracts `database/database.arz` and the
   `gdx1..gdx3` ARZ files to a temp dir, reads `*.dbr` records, caches them, then deletes the temp
   files unless "keep extracted DB files" is set.
4. `BuildIndex(characters, callback)` is implemented per engine and returns `IndexSummary`
   (entries, rarities, types, characters) used to populate the UI filter lists.

Two engines, selected by `SearchEngine` ("Classic" or "Lucene"):

- **Classic** (`Index.cs`): in-memory `List<IndexItem>`, substring match on a prebuilt
  `Searchable` string, `FilterMatch` for filtering. Simple, exact-substring semantics.
- **Lucene** (`LuceneIndex.cs`): `RAMDirectory` index; name terms use prefix + infix wildcards,
  filters become numeric/term queries, duplicates are precomputed into fields
  (`duplicatesOnNormal`, `duplicatesOnHardcoreLiving`, `duplicatesOnHardcoreAll`).

Both share the same item-building logic shape (resolve `itemStatDef`/pet bonus, rarity, type,
level requirement, stats, searchable strings). **If you change item/filter semantics, update both
engines and add tests for both** (`IndexTests` uses the Classic engine).

### UI conventions
- MVVM with ReactiveUI: `ViewModelBase` raises property changes; `MainViewModel` uses
  `RaisePropertyChanged` and `ReactiveCommand` with `async` lambdas.
- XAML uses compiled bindings. `ViewLocator` maps view models to views.
- Property changes on searchable fields (`SearchMode`, level range, `SearchString`, `ItemTypes`,
  `ItemQualities`, etc.) automatically trigger `SearchAsync` — see `SearchablePropertyChanged`.
- `SearchResults` are lazily paged in batches of 100 via `LoadMoreItems` (wired to scroll).
- CJK item names use the bundled `avares://GrimSearch/Assets/Fonts#Noto Sans CJK JP` when the
  selected language is JA/ZH/KO; other languages use the default font.
- `MainWindow.axaml.cs` and `Views/` handle window-level concerns; keep business logic in the VM.

### Steam/GOG detection
`GrimDawnFolderDetectionHelper` detects install/save directories. Windows uses the registry
(Steam path/active user, GOG keys); Linux checks several Steam root locations and parses VDF
files (`VdfFileReader`, `SteamConfig`, `SteamRegistryConfig`). Install locations are validated by
the presence of `ArchiveTool.exe`; save locations by a `main` subdirectory. Unsupported OSes throw
`NotSupportedException`, and detection errors are surfaced to the user, not swallowed.

## Conventions & gotchas

- **Nullable is disabled** in the app project; don't enable it project-wide as a drive-by change.
- `GrimSearch.Utils` uses `AllowUnsafeBlocks` (binary parsers). Existing binary readers use
  manual offsets/`BinaryReader`; keep that style when extending save/archive parsing.
- Archive parsing in `ArzExtractor` is a loose translation of atom0s' `grimarc`/`grimarz`; keep
  the attribution comment if you touch it.
- Binary save formats are versioned (see `Resources/Saves/1.3.0` vs `main/...1.0.7.0`). Add
  fixtures for new game versions rather than breaking older parsing.
- New customer-visible strings/logging: prefer structured NLog logging (`_logger.Info(...)`), and
  remember `StatusBarText` writes are logged as info.
- Keep changes consistent with existing style: explicit `RaisePropertyChanged("Name")` calls,
  file-scoped vs block namespaces vary by folder (match the surrounding file), 4-space indentation.
- The repo publishes install scripts as release assets; `installscripts/install-linux.sh` and
  `README.md` there are user-facing — update them together with installer behavior changes.

## CI / releases

`.github/workflows/build.yaml` ("build GDSearch") runs on PRs to `master`, pushes to `master`,
tags, and manual dispatch:

1. Install GitVersion, checkout with full history, determine version.
2. Setup .NET 10; `dotnet restore && dotnet build --no-restore`; `dotnet test --no-restore --no-build`.
3. `dotnet publish` self-contained for `linux-x64` and `win-x64`, zipped.
4. On tag pushes, `softprops/action-gh-release` publishes both zips, the Linux installer, icon,
   desktop file, and LICENSE.

Before opening a PR: make sure `dotnet build` and `dotnet test` pass locally. Dependency bumps are
usually automated by Renovate (`renovate.json`, grouping non-major updates).
