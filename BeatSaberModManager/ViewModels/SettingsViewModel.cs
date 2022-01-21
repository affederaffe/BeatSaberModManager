using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<string?> _installDir;
        private readonly ObservableAsPropertyHelper<string?> _themesDir;
        private readonly ObservableAsPropertyHelper<bool> _hasValidatedInstallDir;
        private readonly ObservableAsPropertyHelper<bool> _hasThemesDir;

        private const string kBeatSaverProtocol = "beatsaver";
        private const string kModelSaberProtocol = "modelsaber";
        private const string kPlaylistProtocol = "bsplaylist";

        public SettingsViewModel(ModsViewModel modsViewModel, IOptions<AppSettings> appSettings, IInstallDirValidator installDirValidator, IProtocolHandlerRegistrar protocolHandlerRegistrar, IStatusProgress statusProgress, PlaylistInstaller playlistInstaller)
        {
            _appSettings = appSettings;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _playlistInstaller = playlistInstaller;
            ModsViewModel = modsViewModel;
            StatusProgress = (StatusProgress)statusProgress;
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            appSettings.Value.InstallDir.Changed.ToProperty(this, nameof(InstallDir), out _installDir);
            appSettings.Value.InstallDir.Changed.Select(installDirValidator.ValidateInstallDir).ToProperty(this, nameof(HasValidatedInstallDir), out _hasValidatedInstallDir);
            appSettings.Value.ThemesDir.Changed.ToProperty(this, nameof(ThemesDir), out _themesDir);
            appSettings.Value.ThemesDir.Changed.Select(Directory.Exists).ToProperty(this, nameof(HasThemesDir), out _hasThemesDir);
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(kBeatSaverProtocol);
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(kModelSaberProtocol);
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(kPlaylistProtocol);
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, kBeatSaverProtocol));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, kModelSaberProtocol));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, kPlaylistProtocol));
        }

        public ModsViewModel ModsViewModel { get; }

        public StatusProgress StatusProgress { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public bool HasValidatedInstallDir => _hasValidatedInstallDir.Value;

        public bool HasThemesDir => _hasThemesDir.Value;

        public string? InstallDir
        {
            get => _installDir.Value;
            set => _appSettings.Value.InstallDir.Value = value;
        }

        public string? ThemesDir
        {
            get => _themesDir.Value;
            set => _appSettings.Value.ThemesDir.Value = value;
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

        public Task<bool> InstallPlaylistAsync(string path) =>
            _playlistInstaller.InstallPlaylistAsync(_appSettings.Value.InstallDir.Value!, path, StatusProgress);

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }
    }
}