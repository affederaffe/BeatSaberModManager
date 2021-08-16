using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.DependencyInjection;
using BeatSaberModManager.Models.Implementations;
using BeatSaberModManager.Views;

using Splat;


namespace BeatSaberModManager
{
    public class App : Application
    {
        public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();

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
            desktop.MainWindow = desktop.Args.Length > 1 && desktop.Args[0] == "--install"
                ? new AssetInstallWindow(desktop.Args[1])
                : new MainWindow();
        }
    }
}