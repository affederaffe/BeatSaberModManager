using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;
using BeatSaberModManager.Views.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<bool> _hasValidatedInstallDir;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        private const string kBeatSaverProtocol = "beatsaver";
        private const string kModelSaberProtocol = "modelsaber";
        private const string kPlaylistProtocol = "bsplaylist";

        public SettingsViewModel(ModsViewModel modsViewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IProtocolHandlerRegistrar protocolHandlerRegistrar, ILocalisationManager localisationManager, IThemeManager themeManager, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _playlistInstaller = playlistInstaller;
            _beatSaverOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kModelSaberProtocol);
            _modelSaberOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kBeatSaverProtocol);
            _playlistOneClickCheckBoxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kPlaylistProtocol);
            LocalisationManager = localisationManager;
            ThemeManager = themeManager;
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(InstallDir!));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(ThemesDir!));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            appSettings.Value.InstallDir.Changed.Select(installDirValidator.ValidateInstallDir).ToProperty(this, nameof(HasValidatedInstallDir), out _hasValidatedInstallDir);
            appSettings.Value.ThemesDir.Changed.Select(Directory.Exists).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
            this.WhenAnyValue(x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, kBeatSaverProtocol));
            this.WhenAnyValue(x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, kModelSaberProtocol));
            this.WhenAnyValue(x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, kPlaylistProtocol));
            this.WhenAnyValue(x => x.InstallDir).Where(installDirValidator.ValidateInstallDir).BindTo(appSettings, x => x.Value.InstallDir.Value);
            this.WhenAnyValue(x => x.ThemesDir).Where(Directory.Exists).BindTo(appSettings, x => x.Value.ThemesDir.Value);
        }

        public ILocalisationManager LocalisationManager { get; }

        public IThemeManager ThemeManager { get; }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public bool HasValidatedInstallDir => _hasValidatedInstallDir.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        private string? _installDir;
        public string? InstallDir
        {
            get => _appSettings.Value.InstallDir.Value;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _appSettings.Value.ThemesDir.Value;
            set => this.RaiseAndSetIfChanged(ref _themesDir, value);
        }

        public bool ForceReinstallMods
        {
            get => _appSettings.Value.ForceReinstallMods;
            set => _appSettings.Value.ForceReinstallMods = value;
        }

        private bool _beatSaverOneClickCheckboxChecked;
        public bool BeatSaverOneClickCheckboxChecked
        {
            get => _beatSaverOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _beatSaverOneClickCheckboxChecked, value);
        }

        private bool _modelSaberOneClickCheckboxChecked;
        public bool ModelSaberOneClickCheckboxChecked
        {
            get => _modelSaberOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _modelSaberOneClickCheckboxChecked, value);
        }

        private bool _playlistOneClickCheckBoxChecked;
        public bool PlaylistOneClickCheckBoxChecked
        {
            get => _playlistOneClickCheckBoxChecked;
            set => this.RaiseAndSetIfChanged(ref _playlistOneClickCheckBoxChecked, value);
        }

        public Task<bool> InstallPlaylistAsync(string path, IStatusProgress progress) =>
            _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir.Value!, path, progress);

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }
    }
}