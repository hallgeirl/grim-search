using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
        LogManager.Configuration.Variables["logdirectory"] = ConfigFileHelper.GetConfigFolder();

        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly?.GetName().Version?.ToString()
            ?? "unknown";
        LogManager.GetCurrentClassLogger().Info(
            "Starting GrimSearch {Version} ({Framework}, {OS}, {Architecture})",
            version,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture);

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
