using GrimSearch.Common;
using GrimSearch.Utils;
using GrimSearch.Utils.DBFiles;
using GrimSearch.ViewModels;
using log4net;
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

namespace GrimSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = this.DataContext as MainViewModel;
            _viewModel.SettingsMissing += _viewModel_SettingsMissing;
            _viewModel.ErrorOccured += _viewModel_ErrorOccured;
            _viewModel.Dispatcher = Dispatcher;
            LogHelper.GetLog().Debug("Main window initialized.");
        }

        private void _viewModel_ErrorOccured(object sender, ErrorOccuredEventArgs e)
        {
            DisplayError(e.ErrorMessage, e.Exception);
        }

        private void _viewModel_SettingsMissing(object sender, EventArgs e)
        {
            SettingsTab.IsSelected = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResultsListView.PreviewMouseWheel += ResultsListView_PreviewMouseWheel;

            await _viewModel.Initialize();
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

        private void ResultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = ResultsListView.SelectedItem as ItemViewModel;

            if (selected == null)
                return;

            var url = GetGDToolsURLForItem(selected);
            Process.Start(url);
        }

        private string GetGDToolsURLForItem(ItemViewModel item)
        {
            var searchNameInURL = HttpUtility.UrlEncode(item.Name);
            return "https://www.grimtools.com/db/search?query=" + searchNameInURL + "&in_description=0&exact_match=1";
        }

        private void ResultsListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.C)
                CopySelectedValuesToClipboard();
        }

        private void CopySelectedValuesToClipboard()
        {
            var builder = new StringBuilder();
            foreach (ItemViewModel item in ResultsListView.SelectedItems)
            {
                var url = GetGDToolsURLForItem(item);
                builder.AppendLine(item.Name);
            }

            Clipboard.SetText(builder.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.SaveSettingsAsync(true).Wait();
        }
    }
}
