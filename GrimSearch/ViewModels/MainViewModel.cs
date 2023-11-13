using GrimSearch.Common;
using GrimSearch.Utils;
using GrimSearch.Utils.DBFiles;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Index = GrimSearch.Utils.Index;
using ReactiveUI;
using GrimSearch.Views;
using static GrimSearch.Views.MessageBox;
using GrimSearch.Utils.Steam;

namespace GrimSearch.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel(Dispatcher dispatcher) : this()
        {
            Dispatcher = dispatcher;
        }

        public MainViewModel()
        {
            SearchCommand = ReactiveCommand.Create(async () =>
            {
                await SearchAsync();
            });

            SaveSettingsCommand = ReactiveCommand.Create(async () =>
            {
                await SaveSettingsAsync();
            });

            ClearCacheCommand = ReactiveCommand.Create(() =>
            {
                ClearCache();
            });

            RefreshCommand = ReactiveCommand.Create(() =>
            {
                Refresh();
            });
            DetectGDSettingsCommand = ReactiveCommand.Create(async () =>
            {
                await TryDetectGDSettings();
            });

            UpdateSearchBoxVisibilityCommand = ReactiveCommand.Create(() =>
            {
                UpdateSearchBoxVisibility();
            });
        }

        public async Task Initialize()
        {
            await LoadSettingsAsync();
        }

        bool _initialized = false;
        private Index _index = new Index();
        const string settingsFile = "GDItemSearchSettings.json";
        StoredSettings _loadedSettings = new StoredSettings();

        #region Events

        public event EventHandler<ErrorOccuredEventArgs> ErrorOccured;

        private void FireErrorOccured(string text, Exception exception)
        {
            ErrorOccured?.Invoke(this, new ErrorOccuredEventArgs() { ErrorMessage = text, Exception = exception });
        }

        public event EventHandler SettingsMissing;

        #endregion

        #region UI properties
        private string _statusBarText = "";
        public string StatusBarText
        {
            get
            {
                return _statusBarText;
            }
            set
            {
                _statusBarText = value;
                this.RaisePropertyChanged("StatusBarText");
            }
        }

        public Dispatcher Dispatcher;

        public bool AutoRefresh
        {
            get { return _loadedSettings.AutoRefresh; }
            set { _loadedSettings.AutoRefresh = value; this.RaisePropertyChanged("AutoRefresh"); }
        }

        private bool _enableInput = true;
        public bool EnableInput
        {
            get { return _enableInput; }
            set { _enableInput = value; this.RaisePropertyChanged("EnableInput"); }
        }

        private ObservableCollection<string> _searchModes = new ObservableCollection<string> { "Regular", "Find duplicates", "Find new items" };
        public ObservableCollection<string> SearchModes
        {
            get { return _searchModes; }
            set { _searchModes = value; this.RaisePropertyChanged("SearchModes"); }
        }


        private ObservableCollection<MultiselectComboItem> _itemTypes = new ObservableCollection<MultiselectComboItem>();
        public ObservableCollection<MultiselectComboItem> ItemTypes
        {
            get { return _itemTypes; }
            set { _itemTypes = value; this.RaisePropertyChanged("ItemTypes"); }
        }


        private ObservableCollection<MultiselectComboItem> _itemQualities;
        public ObservableCollection<MultiselectComboItem> ItemQualities
        {
            get { return _itemQualities; }
            set { _itemQualities = value; this.RaisePropertyChanged("ItemQualities"); }
        }

        private ObservableCollection<string> _allCharacters = new ObservableCollection<string>() { "(select character)" };
        public ObservableCollection<string> AllCharacters
        {
            get { return _allCharacters; }
            set { _allCharacters = value; this.RaisePropertyChanged("AllCharacters"); }
        }


        private ObservableCollection<ItemViewModel> _searchResults = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> SearchResults
        {
            get { return _searchResults; }
            set { _searchResults = value; this.RaisePropertyChanged("SearchResults"); }
        }

        #endregion

        #region Search filters
        private bool _freeTextSearchVisibility = true;
        public bool FreeTextSearchVisibility
        {
            get { return _freeTextSearchVisibility; }
            set
            {
                _freeTextSearchVisibility = value;
                this.RaisePropertyChanged("FreeTextSearchVisibility");
            }
        }

        private bool _characterBasedSearchVisibility = true;
        public bool CharacterBasedSearchVisibility
        {
            get { return _characterBasedSearchVisibility; }
            set
            {
                _characterBasedSearchVisibility = value;
                this.RaisePropertyChanged("CharacterBasedSearchVisibility");
            }
        }


        public string SearchMode
        {
            get { return _loadedSettings.LastSearchMode; }
            set
            {
                _loadedSettings.LastSearchMode = value;
                this.RaisePropertyChanged("SearchMode");
            }
        }

        private int _minimumLevel = 0;
        public int MinimumLevel
        {
            get { return _minimumLevel; }
            set
            {
                _minimumLevel = value;
                this.RaisePropertyChanged("MinimumLevel");
            }
        }


        private int _maximumLevel = 101;
        public int MaximumLevel
        {
            get { return _maximumLevel; }
            set
            {
                _maximumLevel = value;
                this.RaisePropertyChanged("MaximumLevel");
            }
        }


        private bool _showEquipped;
        public bool ShowEquipped
        {
            get { return _showEquipped; }
            set
            {
                _showEquipped = value;
                this.RaisePropertyChanged("ShowEquipped");
            }
        }

        private string _searchString;
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                this.RaisePropertyChanged("SearchString");
            }
        }

        private string _searchResultText;
        public string SearchResultText
        {
            get
            {
                return _searchResultText;
            }
            set
            {
                _searchResultText = value;
                this.RaisePropertyChanged("SearchResultText");
            }
        }


        #endregion

        #region Settings


        public string GrimDawnDirectory
        {
            get { return _loadedSettings.GrimDawnDirectory; }
            set { _loadedSettings.GrimDawnDirectory = value; this.RaisePropertyChanged("GrimDawnDirectory"); }
        }


        private FileSystemWatcher _savesWatcher = null;
        public string GrimDawnSavesDirectory
        {
            get { return _loadedSettings.SavesDirectory; }
            set
            {
                _loadedSettings.SavesDirectory = value;

                this.RaisePropertyChanged("GrimDawnSavesDirectory");
                WatchDirectory(value);
            }
        }

        private void WatchDirectory(string value)
        {
            if (_savesWatcher != null)
                _savesWatcher.Dispose();

            if (!Directory.Exists(value))
                return;

            _savesWatcher = new FileSystemWatcher(value);
            _savesWatcher.IncludeSubdirectories = true;
            _savesWatcher.EnableRaisingEvents = true;
            _savesWatcher.Changed += _savesWatcher_Changed;
        }

        static object _savesLock = new object();
        static bool _shouldRefresh = false;
        static DateTimeOffset _lastRefreshed = new DateTime(2000, 1, 1);
        private void _savesWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Dispatcher == null || !AutoRefresh)
                return;

            //don't refresh more than every 5 seconds
            if ((DateTime.Now - _lastRefreshed).Seconds < 5)
                return;

            if (_shouldRefresh) //already triggered a refresh - don't trigger another one
                return;

            _shouldRefresh = true;
            Thread.Sleep(3000); //wait 3 seconds to not interfere with saving

            lock (_savesLock)
            {
                //another thread refreshed before this one
                if (!_shouldRefresh)
                    return;

                Dispatcher.Invoke(() =>
                {
                    Refresh();
                });
                _shouldRefresh = false;
                _lastRefreshed = DateTime.Now;
            }
        }

        #endregion

        #region commands

        public ICommand SaveSettingsCommand { get; set; }
        public ICommand ClearCacheCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand SearchAndSaveCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand DetectGDSettingsCommand { get; set; }
        public ICommand UpdateSearchBoxVisibilityCommand { get; set; }


        #endregion

        #region Settings
        public async Task SaveSettingsAsync(bool skipIndexBuild = false)
        {
            var storedSettings = new StoredSettings()
            {
                GrimDawnDirectory = GrimDawnDirectory,
                SavesDirectory = GrimDawnSavesDirectory,
                AutoRefresh = AutoRefresh,
                LastSearchMode = SearchMode,
                LastSearchText = SearchString,
                KeepExtractedDBFiles = _loadedSettings.KeepExtractedDBFiles
            };
            try
            {
                StatusBarText = "Saving settings...";
                EnableInput = false;
                File.WriteAllText(settingsFile, JsonConvert.SerializeObject(storedSettings));

                if (!skipIndexBuild)
                    await BuildIndexAsync();

                _initialized = true;
            }
            catch (Exception ex)
            {
                FireErrorOccured("An error occured while saving settings.", ex);
            }
            finally
            {
                ResetStatusBarText();
                EnableInput = true;
            }
        }

        private async Task LoadSettingsAsync()
        {
            if (File.Exists(settingsFile))
            {
                try
                {
                    StatusBarText = "Loading settings...";
                    _loadedSettings = JsonConvert.DeserializeObject<StoredSettings>(File.ReadAllText(settingsFile));
                    GrimDawnDirectory = _loadedSettings.GrimDawnDirectory;
                    GrimDawnSavesDirectory = _loadedSettings.SavesDirectory;
                    AutoRefresh = _loadedSettings.AutoRefresh;
                    SearchMode = _loadedSettings.LastSearchMode;
                    SearchString = _loadedSettings.LastSearchText;

                    await BuildIndexAsync();

                    _initialized = true;

                    await SearchAsync();
                }
                catch (Exception ex)
                {
                    FireErrorOccured("An error occured while loading settings.", ex);
                }
                finally
                {
                    ResetStatusBarText();
                }

            }
            else
            {
                _loadedSettings = new StoredSettings()
                {
                    AutoRefresh = false,
                    GrimDawnDirectory = "",
                    SavesDirectory = "",
                    LastSearchMode = "Regular",
                    KeepExtractedDBFiles = false,
                    LastSearchText = ""
                };

                GrimDawnDirectory = "";
                GrimDawnSavesDirectory = "";
                SearchMode = "Regular";

                SettingsMissing?.Invoke(this, new EventArgs());
            }
        }


        private void ClearCache()
        {
            SetStatusbarText("Clearing cache...");
            _index.ClearCache();
            ResetStatusBarText();
        }

        private void ResetStatusBarText()
        {
            SetStatusbarText("Ready");
        }

        private void SetStatusbarText(string text)
        {
            if (Dispatcher == null)
                return;
            Dispatcher.Invoke(() =>
            {
                StatusBarText = text;
            });
        }

        private async Task TryDetectGDSettings()
        {
            string errors = "";

            var gdFolderDetect = new GrimDawnWindowsFolderDetectionHelper();
            try
            {
                GrimDawnSavesDirectory = gdFolderDetect.DetectGrimDawnSavesDirectory();
            }
            catch (Exception ex)
            {
                errors += ex.Message;
            }

            try
            {
                GrimDawnDirectory = gdFolderDetect.DetectGrimDawnDirectory();
            }
            catch (Exception ex)
            {
                errors += ex.Message;
            }

            if (!string.IsNullOrEmpty(errors))
                throw new InvalidOperationException(errors);
            else
            {
                await Dispatcher.Invoke(async () =>
                {
                    //TODO
                    //var answer = MessageBox.Show(this, "The Grim Dawn directories have been successfully detected. Do you want to save and start loading items and characters?", "Success", MessageBoxButtons.YesNo);
                    //if (answer == MessageBoxResult.Yes)
                    //    await SaveSettingsAsync();
                });
            }
        }

        #endregion

        #region Search and index

        private async Task BuildIndexAsync(bool skipItemTypesReload = false)
        {
            SetStatusbarText("Loading characters and items...");
            IndexSummary result;

            var newIndex = new Index();
            result = await newIndex.BuildAsync(GrimDawnDirectory, GrimDawnSavesDirectory, _loadedSettings.KeepExtractedDBFiles, false, (msg) => SetStatusbarText(msg)).ConfigureAwait(false);
            _index = newIndex;

            if (!skipItemTypesReload)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    var itemQualities = new ObservableCollection<MultiselectComboItem>();
                    itemQualities.AddRange(result.ItemRarities.Select(x => new MultiselectComboItem() { Selected = (x != "Common" && x != "Rare" && x != "Magical"), Value = x, DisplayText = x }));
                    ItemQualities = itemQualities;

                    ItemTypes.Clear();
                    ItemTypes.AddRange(result.ItemTypes.Select(x => new MultiselectComboItem() { Selected = true, Value = x, DisplayText = ItemHelper.GetItemTypeDisplayName(x) }));

                    AllCharacters.Clear();
                    AllCharacters.Add("(select character)");
                    AllCharacters.AddRange(result.Characters);
                }));
            }
        }


        private async Task SearchAsync()
        {
            if (!_initialized)
                return;

            var filter = CreateIndexFilter();
            SetStatusbarText("Searching for " + SearchString);

            SearchResult items;

            if (SearchMode == "Find duplicates")
                items = await _index.FindDuplicatesAsync(SearchString, filter).ConfigureAwait(false);
            else if (SearchMode == "Find new items")
                items = await _index.FindUniqueAsync(SearchString, filter).ConfigureAwait(false);
            else
                items = await _index.FindAsync(SearchString, filter).ConfigureAwait(false);

            Dispatcher.Invoke(() =>
            {
                SearchResults.Clear();
                SearchResults.AddRange(items.Results.Select(x => ItemViewModel.FromModel(x)));

                SearchResultText = "(showing " + items.Results.Count + " of " + items.TotalCount + ")";
            });

            ResetStatusBarText();
        }

        private IndexFilter CreateIndexFilter()
        {
            IndexFilter filter = new IndexFilter();
            filter.MinLevel = MinimumLevel;
            filter.MaxLevel = MaximumLevel;

            filter.IncludeEquipped = ShowEquipped;

            var rarityItems = ItemQualities;
            if (rarityItems != null)
                filter.ItemQualities = rarityItems.Where(x => x.Selected).Select(x => x.Value).ToArray();

            var itemTypes = ItemTypes as IEnumerable<MultiselectComboItem>;
            if (itemTypes != null)
                filter.ItemTypes = itemTypes.Where(x => x.Selected).Select(x => x.Value).ToArray();

            filter.PageSize = 50;
            return filter;
        }

        private async void Refresh()
        {
            SetStatusbarText("Refreshing...");
            await BuildIndexAsync(true).ConfigureAwait(false);
            await SearchAsync().ConfigureAwait(false);
            ResetStatusBarText();
        }

        private void UpdateSearchBoxVisibility()
        {
            // use only search textbox for now
            FreeTextSearchVisibility = true;
            CharacterBasedSearchVisibility = true;

            /*
            if (SearchMode == "Regular")
            {
                FreeTextSearchVisibility = Visibility.Visible;
                CharacterBasedSearchVisibility = Visibility.Hidden;
            }
            else
            {
                FreeTextSearchVisibility = Visibility.Hidden;
                CharacterBasedSearchVisibility = Visibility.Visible;
            }*/

        }
        #endregion
    }
}
