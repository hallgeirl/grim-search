using GDItemSearch.Common;
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
        MainViewModel _viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = this.DataContext as MainViewModel;
            _viewModel.SettingsMissing += _viewModel_SettingsMissing;
            _viewModel.ErrorOccured += _viewModel_ErrorOccured;
        }

        private void _viewModel_ErrorOccured(object sender, ErrorOccuredEventArgs e)
        {
            DisplayError(e.ErrorMessage, e.Exception);
        }

        private void _viewModel_SettingsMissing(object sender, EventArgs e)
        {
            SettingsTab.IsSelected = true;
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            var searchNameInURL = HttpUtility.UrlEncode(selected.Name);
            Process.Start("https://www.grimtools.com/db/search?query=" + searchNameInURL + "&in_description=0&exact_match=1");
        }
    }
}
