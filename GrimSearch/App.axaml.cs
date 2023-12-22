using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GrimSearch.Utils;
using GrimSearch.ViewModels;
using GrimSearch.Views;
using NLog;

namespace GrimSearch;

public partial class App : Application
{
    public override void Initialize()
    {
        NLog.LogManager.Setup();
        string assemblyFolder = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        LogManager.Configuration.Variables["logdirectory"] = ConfigFileHelper.GetConfigFolder();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext = new MainViewModel(Dispatcher.UIThread, desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}

