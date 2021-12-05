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
        private readonly ISettings<AppSettings> _appSettings;
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
            _appSettings = appSettings;
            _progress = progress;
            _installDirValidator = installDirValidator;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _playlistInstaller = playlistInstaller;
            _beatSaverOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kModelSaberProtocol);
            _modelSaberOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kBeatSaverProtocol);
            _playlistOneClickCheckBoxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kPlaylistProtocol);
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(InstallDir!), this.WhenAnyValue(x => x.InstallDir).Select(Directory.Exists));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(ThemesDir!), this.WhenAnyValue(x => x.ThemesDir).Select(Directory.Exists));
            IObservable<bool> isInstallDirValidObservable = appSettings.Value.InstallDir.Changed.Select(installDirValidator.ValidateInstallDir);
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync, isInstallDirValidObservable);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync, isInstallDirValidObservable);
            this.WhenAnyValue(x => x.BeatSaverOneClickCheckboxChecked).Subscribe(b => ToggleOneClickHandler(b, kBeatSaverProtocol));
            this.WhenAnyValue(x => x.ModelSaberOneClickCheckboxChecked).Subscribe(b => ToggleOneClickHandler(b, kModelSaberProtocol));
            this.WhenAnyValue(x => x.PlaylistOneClickCheckBoxChecked).Subscribe(b => ToggleOneClickHandler(b, kPlaylistProtocol));
            isInstallDirValidObservable.ToProperty(this, nameof(HasValidatedInstallDir), out _hasValidatedInstallDir);
            appSettings.Value.ThemesDir.Changed.Select(Directory.Exists).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public bool HasValidatedInstallDir => _hasValidatedInstallDir.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        public string? InstallDir
        {
            get => _appSettings.Value.InstallDir.Value;
            set => ValidateAndSetInstallDir(value);
        }

        public string? ThemesDir
        {
            get => _appSettings.Value.ThemesDir.Value;
            set => ValidateAndSetThemesDir(value);
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

        public async Task InstallPlaylistsAsync(string path)
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return;
            await _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir.Value!, path, _progress).ConfigureAwait(false);
        }

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }

        private void ValidateAndSetInstallDir(string? value)
        {
            if (!_installDirValidator.ValidateInstallDir(value)) return;
            this.RaisePropertyChanging(nameof(InstallDir));
            _appSettings.Value.InstallDir.Value = value;
            this.RaisePropertyChanged(nameof(InstallDir));
        }

        private void ValidateAndSetThemesDir(string? value)
        {
            if (!Directory.Exists(value)) return;
            this.RaisePropertyChanging(nameof(ThemesDir));
            _appSettings.Value.ThemesDir.Value = value;
            this.RaisePropertyChanged(nameof(ThemesDir));
        }
    }
}