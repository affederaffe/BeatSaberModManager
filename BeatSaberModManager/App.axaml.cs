using Avalonia;

using Avalonia.Markup.Xaml;

using BeatSaberModManager.Localisation;
using BeatSaberModManager.Theming;


namespace BeatSaberModManager
{
    public class App : Application
    {
        private readonly LocalisationManager _localisationManager = null!;
        private readonly ThemeManager _themeManager = null!;

        public App() { }

        public App(LocalisationManager localisationManager, ThemeManager themeManager)
        {
            _localisationManager = localisationManager;
            _themeManager = themeManager;
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localisationManager.Initialize(this);
            _themeManager.Initialize(this);
        }
    }
}