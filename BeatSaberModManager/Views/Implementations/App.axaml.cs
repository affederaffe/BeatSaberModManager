using System;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Views.Implementations.Windows;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations
{
    public class App : Application
    {
        private readonly AppSettings _appSettings = null!;
        private readonly IClassicDesktopStyleApplicationLifetime _lifetime = null!;
        private readonly ILocalisationManager _localisationManager = null!;
        private readonly IThemeManager _themeManager = null!;
        private readonly IInstallDirValidator _installDirValidator = null!;
        private readonly IInstallDirLocator _installDirLocator = null!;

        public App() { }

        [ActivatorUtilitiesConstructor]
        public App(IServiceProvider services, ISettings<AppSettings> appSettings, IClassicDesktopStyleApplicationLifetime lifetime, ILocalisationManager localisationManager, IThemeManager themeManager, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _appSettings = appSettings.Value;
            _lifetime = lifetime;
            _localisationManager = localisationManager;
            _themeManager = themeManager;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            DataTemplates.Add(new ViewLocator(services));
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _localisationManager.Initialize();
            _themeManager.Initialize();
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (_installDirValidator.ValidateInstallDir(_appSettings.InstallDir.Value)) return;
            _appSettings.InstallDir.Value = _installDirLocator.LocateInstallDir();
            if (_installDirValidator.ValidateInstallDir(_appSettings.InstallDir.Value)) return;
            _appSettings.InstallDir.Value = await new InstallFolderDialogWindow().ShowDialog<string?>(_lifetime.MainWindow).ConfigureAwait(false);
        }
    }
}