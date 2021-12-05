using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Views.Implementations.Localisation;
using BeatSaberModManager.Views.Implementations.Theming;
using BeatSaberModManager.Views.Implementations.Windows;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations
{
    public class App : Application
    {
        private readonly IServiceProvider _services = null!;
        private readonly ISettings<AppSettings> _appSettings = null!;
        private readonly ILocalisationManager _localisationManager = null!;
        private readonly IThemeManager _themeManager = null!;
        private readonly IInstallDirValidator _installDirValidator = null!;
        private readonly IInstallDirLocator _installDirLocator = null!;

        public App() { }

        [ActivatorUtilitiesConstructor]
        public App(IServiceProvider services, ISettings<AppSettings> appSettings, ILocalisationManager localisationManager, IThemeManager themeManager, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _services = services;
            _appSettings = appSettings;
            _localisationManager = localisationManager;
            _themeManager = themeManager;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            DataTemplates.Add(new ViewLocator(services));
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localisationManager.Initialize(l => Resources.MergedDictionaries[0] = ((Language)l).ResourceProvider);
            _themeManager.Initialize(t => Styles[0] = ((Theme)t).Style);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;
            lifetime.MainWindow = _services.GetRequiredService<Window>();
            if (_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return;
            _appSettings.Value.InstallDir.Value = _installDirLocator.LocateInstallDir();
            if (_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return;
            _appSettings.Value.InstallDir.Value = await new InstallFolderDialogWindow().ShowDialog<string?>(lifetime.MainWindow);
        }
    }
}