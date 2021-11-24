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

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AppSettings _appSettings;
        private readonly IStatusProgress _progress;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<bool> _hasValidatedInstallDir;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        private const string kBeatSaverProtocol = "beatsaver";
        private const string kModelSaberProtocol = "modelsaber";
        private const string kPlaylistProtocol = "bsplaylist";

        public SettingsViewModel(ModsViewModel modsViewModel, ISettings<AppSettings> appSettings, IStatusProgress progress, IInstallDirValidator installDirValidator, IProtocolHandlerRegistrar protocolHandlerRegistrar, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings.Value;
            _progress = progress;
            _installDirValidator = installDirValidator;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _playlistInstaller = playlistInstaller;
            _beatSaverOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kModelSaberProtocol);
            _modelSaberOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kBeatSaverProtocol);
            _playlistOneClickCheckBoxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kPlaylistProtocol);
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(InstallDir!), this.WhenAnyValue(x => x.InstallDir).Select(Directory.Exists));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(ThemesDir!), this.WhenAnyValue(x => x.ThemesDir).Select(Directory.Exists));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            this.WhenAnyValue(x => x.BeatSaverOneClickCheckboxChecked).Subscribe(b => ToggleOneClickHandler(b, kBeatSaverProtocol));
            this.WhenAnyValue(x => x.ModelSaberOneClickCheckboxChecked).Subscribe(b => ToggleOneClickHandler(b, kModelSaberProtocol));
            this.WhenAnyValue(x => x.PlaylistOneClickCheckBoxChecked).Subscribe(b => ToggleOneClickHandler(b, kPlaylistProtocol));
            _appSettings.InstallDir.Changed.Select(installDirValidator.ValidateInstallDir).ToProperty(this, nameof(HasValidatedInstallDir), out _hasValidatedInstallDir);
            _appSettings.ThemesDir.Changed.Select(Directory.Exists).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public bool HasValidatedInstallDir => _hasValidatedInstallDir.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        public string? InstallDir
        {
            get => _appSettings.InstallDir.Value;
            set => ValidateAndSetInstallDir(value);
        }

        public string? ThemesDir
        {
            get => _appSettings.ThemesDir.Value;
            set => ValidateAndSetThemesDir(value);
        }

        public bool ForceReinstallMods
        {
            get => _appSettings.ForceReinstallMods;
            set => _appSettings.ForceReinstallMods = value;
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

        public async Task InstallPlaylistsAsync(string path) => await _playlistInstaller.InstallPlaylistAsync(path, _progress);

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }

        private void ValidateAndSetInstallDir(string? value)
        {
            if (_installDirValidator.ValidateInstallDir(value))
                _appSettings.InstallDir.Value = value;
        }

        private void ValidateAndSetThemesDir(string? value)
        {
            if (Directory.Exists(value))
                _appSettings.ThemesDir.Value = value;
        }
    }
}