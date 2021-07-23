using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.DependencyInjection;
using BeatSaberModManager.Views;

using Splat;


namespace BeatSaberModManager
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void RegisterServices()
        {
            Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);
            base.RegisterServices();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();
        }
    }
}