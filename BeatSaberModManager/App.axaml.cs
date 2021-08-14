using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Theming;

using BeatSaberModManager.DependencyInjection;
using BeatSaberModManager.Localisation;
using BeatSaberModManager.Models;
using BeatSaberModManager.Views;

using Splat;


namespace BeatSaberModManager
{
    public class App : Application
    {
        public static void Main(string[] args) => AppBuilder.Configure<App>().UsePlatformDetect().UseReactiveUI().LogToTrace().StartWithClassicDesktopLifetime(args);

        public override void RegisterServices()
        {
            ServicesBootstrapper.Register(Locator.CurrentMutable, Locator.Current);
            ViewModelBootstrapper.Register(Locator.CurrentMutable, Locator.Current);
            base.RegisterServices();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            Settings settings = Locator.Current.GetService<Settings>();
            desktop.Exit += (_, _) => settings.Save();
            Locator.Current.GetService<LanguageSwitcher>().Initialize(settings.LanguageName);
            Locator.Current.GetService<ThemeSwitcher>().Initialize(settings.ThemeName);
            desktop.MainWindow = desktop.Args.Length > 1 && desktop.Args[0] == "--install"
                ? new AssetInstallWindow(desktop.Args[1])
                : new MainWindow();
        }
    }
}