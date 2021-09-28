using Avalonia;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations
{
    public class App : Application
    {
        private readonly ILocalisationManager _localisationManager = null!;
        private readonly IThemeManager _themeManager = null!;

        public App() { }

        [ActivatorUtilitiesConstructor]
        public App(ILocalisationManager localisationManager, IThemeManager themeManager)
        {
            _localisationManager = localisationManager;
            _themeManager = themeManager;
        }

        public override void RegisterServices()
        {
            AvaloniaLocator.CurrentMutable.BindToSelf(this as Application);
            base.RegisterServices();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localisationManager.Initialize();
            _themeManager.Initialize();
        }
    }
}