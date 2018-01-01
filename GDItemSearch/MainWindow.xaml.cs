using GDItemSearch.FileUtils;
using GDItemSearch.FileUtils.DBFiles;
using GDItemSearch.ViewModels;
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

        public ObservableCollection<MultiselectComboItem> ItemTypes = new ObservableCollection<MultiselectComboItem>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSettings();

            ResultsListView.PreviewMouseWheel += ResultsListView_PreviewMouseWheel;

            ItemTypesSelector.ItemsSource = ItemTypes;
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
                    GDDirTextBox.Text = Settings.GrimDawnDirectory;
                    GDSavesTextBox.Text = Settings.SavesDirectory;

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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.GrimDawnDirectory = GDDirTextBox.Text;
            Settings.SavesDirectory = GDSavesTextBox.Text;

            var storedSettings = new StoredSettings() { GrimDawnDirectory = GDDirTextBox.Text, SavesDirectory = GDSavesTextBox.Text };
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

        private async Task BuildIndex()
        {
            SetStatusBarText("Loading characters and items...");
            var result = await Task.Run<IndexSummary>(() => _index.Build());

            var rarities = new List<MultiselectComboItem>();
            rarities.AddRange(result.ItemRarities.Select(x => new MultiselectComboItem() { Selected = true, Value = x, DisplayText = x }));
            RaritySelector.ItemsSource = rarities;

            ItemTypes.Clear();
            ItemTypes.AddRange(result.ItemTypes.Select(x=>new MultiselectComboItem() { Selected = true, Value = x, DisplayText = ItemHelper.GetItemTypeDisplayName(x) }));
        }

        private void SetStatusBarText(string s)
        {
            StatusBarText.Text = s;
        }

        private void ResetStatusBarText()
        {
            StatusBarText.Text = "Ready";
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
            var searchText = SearchTextBox.Text;
            SetStatusBarText("Searching for " + searchText);

            await Task.Run(() =>
            {
                var items = _index.Find(searchText, filter);

                Dispatcher.Invoke(() =>
                {
                    ResultsListView.ItemsSource = items.Select(x=>ItemViewModel.FromModel(x));
                });
            });
            ResetStatusBarText();
        }

        private IndexFilter CreateIndexFilter()
        {
            IndexFilter filter = new IndexFilter();
            if (!string.IsNullOrEmpty(MinimumLevelTextBox.Text))
                filter.MinLevel = int.Parse(MinimumLevelTextBox.Text);

            if (!string.IsNullOrEmpty(MaximumLevelTextBox.Text))
                filter.MaxLevel = int.Parse(MaximumLevelTextBox.Text);

            if (!IncludeEquippedCheckBox.IsChecked.HasValue || !IncludeEquippedCheckBox.IsChecked.Value)
                filter.IsEquipped = false;

            var rarityItems = RaritySelector.ItemsSource as IEnumerable<MultiselectComboItem>;
            if (rarityItems != null)
                filter.ItemQualities = rarityItems.Where(x => x.Selected).Select(x => x.Value).ToArray();

            var itemTypes = ItemTypesSelector.ItemsSource as IEnumerable<MultiselectComboItem>;
            if (itemTypes != null)
                filter.ItemTypes = itemTypes.Where(x => x.Selected).Select(x => x.Value).ToArray();

            filter.PageSize = 50;
            return filter;
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ResultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = ResultsListView.SelectedItem as ItemViewModel;

            if (selected == null)
                return;

            var searchNameInURL = HttpUtility.UrlEncode(selected.Name);
            Process.Start("https://www.grimtools.com/db/search?query=" + searchNameInURL + "&in_description=0&exact_match=1");
        }
    }
}
