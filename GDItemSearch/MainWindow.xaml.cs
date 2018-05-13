using GDItemSearch.FileUtils;
using GDItemSearch.FileUtils.DBFiles;
using GDItemSearch.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GDItemSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Index _index = new Index();
        const string settingsFile = "GDItemSearchSettings.json";
        bool _initialized = false;
        MainViewModel _viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = this.DataContext as MainViewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSettings();

            ResultsListView.PreviewMouseWheel += ResultsListView_PreviewMouseWheel;
        }

        private void ResultsListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private async Task LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                try
                {
                    SetStatusBarText("Loading settings...");
                    var settings = JsonConvert.DeserializeObject<StoredSettings>(File.ReadAllText(settingsFile));
                    Settings.GrimDawnDirectory = settings.GrimDawnDirectory;
                    Settings.SavesDirectory = settings.SavesDirectory;
                    _viewModel.GrimDawnDirectory = Settings.GrimDawnDirectory;
                    _viewModel.GrimDawnSavesDirectory = Settings.SavesDirectory;

                    await BuildIndex();

                    _initialized = true;

                    await Search();
                }
                catch (Exception ex)
                {
                    DisplayError("An error occured while loading settings.", ex);
                }
                finally
                {
                    ResetStatusBarText();
                }
                
            }
            else
            {
                Settings.GrimDawnDirectory = "";
                Settings.SavesDirectory = "";

                SettingsTab.IsSelected = true;
            }
        }

        private async void DetectGDSettings_Click(object sender, RoutedEventArgs e)
        {
            await TryDetectGDSettings();
        }

        private async Task TryDetectGDSettings()
        {
            string steamPath = GetRegistryValue<string>("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
            int activeUser = GetRegistryValue<int>("HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess", "ActiveUser");

            if (!Directory.Exists(steamPath))
                throw new InvalidOperationException("Steam path was not found. Is it installed?");

            if (activeUser == 0)
                throw new InvalidOperationException("Steam is not running, or you are not logged in.");

            string errors = "";

            string gdDir = System.IO.Path.Combine(steamPath, "SteamApps", "common", "Grim Dawn").Replace('/', '\\');
            string savesDir = System.IO.Path.Combine(steamPath, "userdata", activeUser.ToString(), "219990", "remote", "save").Replace('/', '\\');

            if (!File.Exists(System.IO.Path.Combine(gdDir, "ArchiveTool.exe")))
            {
                errors += "The Grim Dawn directory was not found in the default install location for Steam games. Please specify this manually.";
            }
            else
            {
                _viewModel.GrimDawnDirectory = gdDir;
            }

            if (!Directory.Exists(System.IO.Path.Combine(savesDir, "main")))
            {
                errors += "Grim Dawn saves directory was not found at " + savesDir + ". Please specify this manually.";
            }
            else
            {
                _viewModel.GrimDawnSavesDirectory = savesDir;
            }

            if (!string.IsNullOrEmpty(errors))
                throw new InvalidOperationException(errors);
            else
            {
                var answer = MessageBox.Show("The Grim Dawn directories have been successfully detected. Do you want to save and start loading items and characters?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                    await SaveSettings();
            }
        }

        private T GetRegistryValue<T>(string path, string valueName)
        {
            var value = Registry.GetValue(path, valueName, null);

            if (value == null)
                return default(T);

            return (T)value;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveSettings();
        }

        private async Task SaveSettings()
        {
            Settings.GrimDawnDirectory = _viewModel.GrimDawnDirectory;
            Settings.SavesDirectory = _viewModel.GrimDawnSavesDirectory;

            var storedSettings = new StoredSettings() { GrimDawnDirectory = _viewModel.GrimDawnDirectory, SavesDirectory = _viewModel.GrimDawnSavesDirectory };
            try
            {
                SetStatusBarText("Saving settings...");
                this.IsEnabled = false;
                File.WriteAllText(settingsFile, JsonConvert.SerializeObject(storedSettings));

                SetStatusBarText("Clearing cache...");
                _index.ClearCache();

                await BuildIndex();

                _initialized = true;
            }
            catch (Exception ex)
            {
                DisplayError("An error occured while saving settings.", ex);
            }
            finally
            {
                ResetStatusBarText();
                this.IsEnabled = true;
            }
        }

        private async Task BuildIndex(bool skipItemTypesReload = false)
        {
            SetStatusBarText("Loading characters and items...");
            var result = await Task.Run<IndexSummary>(() => _index.Build());

            if (!skipItemTypesReload)
            {
                var itemQualities = new ObservableCollection<MultiselectComboItem>();
                itemQualities.AddRange(result.ItemRarities.Select(x => new MultiselectComboItem() { Selected = (x != "Common" && x != "Rare" && x != "Magical"), Value = x, DisplayText = x }));
                _viewModel.ItemQualities = itemQualities;

                _viewModel.ItemTypes.Clear();

                _viewModel.ItemTypes.AddRange(result.ItemTypes.Select(x => new MultiselectComboItem() { Selected = true, Value = x, DisplayText = ItemHelper.GetItemTypeDisplayName(x) }));
            }
        }

        private void SetStatusBarText(string s)
        {
            _viewModel.StatusBarText = s;
        }

        private void ResetStatusBarText()
        {
            _viewModel.StatusBarText = "Ready";
        }

        private void DisplayError(string errorMessage, Exception ex)
        {
            var debugMode = ConfigurationManager.AppSettings["debugMode"];

            var debugModeEnabled = false;
            bool.TryParse(debugMode, out debugModeEnabled);

            string exText = ex.Message;
            if (debugModeEnabled)
            {
                exText = ex.ToString();
            }

            MessageBox.Show(errorMessage + " Details: " + exText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private async void Search_CheckboxChecked(object sender, RoutedEventArgs e)
        {
            await Search();
        }

        private async void Search_TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            await Search();
        }

        private async void Search_FilterSelectionChanged(object sender, EventArgs e)
        {
            await Search();
        }

        private async Task Search()
        {
            if (!_initialized)
                return;

            var filter = CreateIndexFilter();
            var searchText = _viewModel.SearchString;
            string searchMode = _viewModel.SearchMode;
            SetStatusBarText("Searching for " + searchText);

            await Task.Run(() =>
            {
                List<IndexItem> items;
                if (searchMode == "Duplicate search")
                    items = _index.FindDuplicates(searchText, filter);
                else
                    items = _index.Find(searchText, filter);

                Dispatcher.Invoke(() =>
                {
                    _viewModel.SearchResults.Clear();
                    _viewModel.SearchResults.AddRange(items.Select(x => ItemViewModel.FromModel(x)));
                });
            });
            ResetStatusBarText();
        }

        private IndexFilter CreateIndexFilter()
        {
            IndexFilter filter = new IndexFilter();
            filter.MinLevel = _viewModel.MinimumLevel;
            filter.MaxLevel = _viewModel.MaximumLevel;

            filter.IsEquipped = _viewModel.ShowEquipped;

            var rarityItems = _viewModel.ItemQualities;
            if (rarityItems != null)
                filter.ItemQualities = rarityItems.Where(x => x.Selected).Select(x => x.Value).ToArray();

            var itemTypes = _viewModel.ItemTypes as IEnumerable<MultiselectComboItem>;
            if (itemTypes != null)
                filter.ItemTypes = itemTypes.Where(x => x.Selected).Select(x => x.Value).ToArray();

            filter.PageSize = 50;
            return filter;
        }

        private void ResultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = ResultsListView.SelectedItem as ItemViewModel;
            
            if (selected == null)
                return;

            var searchNameInURL = HttpUtility.UrlEncode(selected.Name);
            Process.Start("https://www.grimtools.com/db/search?query=" + searchNameInURL + "&in_description=0&exact_match=1");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await BuildIndex(true);
            await Search();
            ResetStatusBarText();
        }
    }
}
