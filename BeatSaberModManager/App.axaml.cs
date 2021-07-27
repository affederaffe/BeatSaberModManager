using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BeatSaberModManager.Theming;

using BeatSaberModManager.DependencyInjection;
using BeatSaberModManager.Localisation;
using BeatSaberModManager.Models;
using BeatSaberModManager.Utils;
using BeatSaberModManager.Views;

using Splat;


namespace BeatSaberModManager
{
    public class App : Application
    {
        public override void RegisterServices()
        {
            ServicesBootstrapper.RegisterServices(Locator.CurrentMutable, Locator.Current);
            ViewModelBootstrapper.RegisterServices(Locator.CurrentMutable, Locator.Current);
            base.RegisterServices();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Settings settings = Locator.Current.GetService<Settings>();
            LanguageSwitcher languageSwitcher = Locator.Current.GetService<LanguageSwitcher>();
            languageSwitcher.SelectedLanguage = languageSwitcher.Languages.FirstOrDefault(x => x.Name == settings.LanguageName) ?? languageSwitcher.Languages.First();
            ThemeSwitcher themeSwitcher = Locator.Current.GetService<ThemeSwitcher>();
            themeSwitcher.SelectedTheme = themeSwitcher.Themes.FirstOrDefault(x => x.Name == settings.ThemeName) ?? themeSwitcher.Themes.First();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            if (desktop.Args.Length > 0)
            {
                switch (desktop.Args[0])
                {
                    case "--register" when desktop.Args.Length > 3:
                        PlatformUtils.RegisterProtocolHandler(desktop.Args[1], desktop.Args[2], desktop.Args[3]);
                        break;
                    case "--unregister" when desktop.Args.Length > 2:
                        PlatformUtils.UnregisterProtocolHandler(desktop.Args[1], desktop.Args[2]);
                        break;
                    case "--install" when desktop.Args.Length > 1:
                        desktop.MainWindow = new AssetInstallWindow { Uri = desktop.Args[1] };
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            else
            {
                desktop.MainWindow = new MainWindow();
            }
        }
    }
}