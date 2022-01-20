using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Theming;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views
{
    public class App : Application
    {
        private readonly IServiceProvider _services = null!;
        private readonly LocalizationManager _localizationManager = null!;
        private readonly ThemeManager _themeManager = null!;

        public App() { }

        [ActivatorUtilitiesConstructor]
        public App(IServiceProvider services, LocalizationManager localizationManager, ThemeManager themeManager)
        {
            _services = services;
            _localizationManager = localizationManager;
            _themeManager = themeManager;
            DataTemplates.Add(new ViewLocator(services));
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localizationManager.Initialize(l => Resources.MergedDictionaries[0] = l.ResourceProvider);
            _themeManager.Initialize(t => Styles[0] = t.Style);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow = _services.GetRequiredService<Window>();
        }
    }
}