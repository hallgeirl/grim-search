using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GrimSearch.Common;
using GrimSearch.ViewModels;
using static GrimSearch.Views.MessageBox;

namespace GrimSearch.Views;

public partial class MainWindow : Window
{
    MainViewModel _viewModel = new MainViewModel();

    public MainWindow()
    {
        InitializeComponent();

        this.Opened += Window_Initialized;
        this.Closing += Window_Closing;
    }

    private async void Window_Initialized(object sender, EventArgs e)
    {
        _viewModel = DataContext as MainViewModel;
        _viewModel.SettingsMissing += _viewModel_SettingsMissing;
        _viewModel.ErrorOccured += _viewModel_ErrorOccured;
        await _viewModel.Initialize();
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
        await _viewModel.Initialize();
    }

    private void DisplayError(string errorMessage, Exception ex)
    {
        //TODO var debugMode = ConfigurationManager.AppSettings["debugMode"];

        var debugModeEnabled = false;
        //TODO bool.TryParse(debugMode, out debugModeEnabled);

        string exText = ex.Message;
        if (debugModeEnabled)
        {
            exText = ex.ToString();
        }

        MessageBox.Show(this, errorMessage + " Details: " + exText, "Error", MessageBoxButtons.Ok);
    }

    private void ResultsListView_MouseDoubleClick(object sender, TappedEventArgs e)
    {
        var selected = ResultsListView.SelectedItem as ItemViewModel;

        if (selected == null)
            return;

        var url = GetGDToolsURLForItem(selected);
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private string GetGDToolsURLForItem(ItemViewModel item)
    {
        var searchNameInURL = Uri.EscapeDataString(item.Name);
        return "https://www.grimtools.com/db/search?query=" + searchNameInURL + "&in_description=0&exact_match=1";
    }

    private async void ResultsListView_KeyUp(object sender, KeyEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Control) != 0 && e.Key == Key.C)
            await CopySelectedValuesToClipboard();
    }

    private async Task CopySelectedValuesToClipboard()
    {
        var builder = new StringBuilder();
        foreach (ItemViewModel item in ResultsListView.SelectedItems)
        {
            var url = GetGDToolsURLForItem(item);
            builder.AppendLine(item.Name);
        }

        await Clipboard.SetTextAsync(builder.ToString());
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.SaveSettingsAsync(true).Wait();
    }

}
