using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Views.Implementations.Localisation;
using BeatSaberModManager.Views.Implementations.Theming;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations
{
    public class App : Application
    {
        private readonly IServiceProvider _services = null!;
        private readonly ILocalisationManager _localisationManager = null!;
        private readonly IThemeManager _themeManager = null!;

        public App() { }

        [ActivatorUtilitiesConstructor]
        public App(IServiceProvider services, ILocalisationManager localisationManager, IThemeManager themeManager)
        {
            _services = services;
            _localisationManager = localisationManager;
            _themeManager = themeManager;
            DataTemplates.Add(new ViewLocator(services));
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localisationManager.Initialize(l => Resources.MergedDictionaries[0] = ((Language)l).ResourceProvider);
            _themeManager.Initialize(t => Styles[0] = ((Theme)t).Style);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow = _services.GetRequiredService<Window>();
        }
    }
}