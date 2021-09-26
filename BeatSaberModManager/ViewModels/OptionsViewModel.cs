using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class OptionsViewModel : ReactiveObject
    {
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;
        private readonly PlaylistInstaller _playlistInstaller;
        private readonly ObservableAsPropertyHelper<bool> _hasValidatedInstallDir;
        private readonly ObservableAsPropertyHelper<bool> _openThemesDirButtonActive;

        private const string kBeatSaverProtocol = "beatsaver";
        private const string kModelSaberProtocol = "modelsaber";
        private const string kPlaylistProtocol = "bsplaylist";

        public OptionsViewModel(ModsViewModel modsViewModel, ISettings<AppSettings> appSettings, IProtocolHandlerRegistrar protocolHandlerRegistrar, PlaylistInstaller playlistInstaller, IInstallDirValidator installDirValidator)
        {
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _playlistInstaller = playlistInstaller;
            _installDir = appSettings.Value.InstallDir;
            _themesDir = appSettings.Value.ThemesDir;
            _beatSaverOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kModelSaberProtocol);
            _modelSaberOneClickCheckboxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kBeatSaverProtocol);
            _playlistOneClickCheckBoxChecked = _protocolHandlerRegistrar.IsProtocolHandlerRegistered(kPlaylistProtocol);
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenFolder(InstallDir));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenFolder(ThemesDir));
            UninstallModLoaderCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallModLoaderAsync);
            UninstallAllModsCommand = ReactiveCommand.CreateFromTask(modsViewModel.UninstallAllModsAsync);
            this.WhenAnyValue(x => x.BeatSaverOneClickCheckboxChecked).Subscribe(b => ToggleOneClickHandler(b, kBeatSaverProtocol));
            this.WhenAnyValue(x => x.ModelSaberOneClickCheckboxChecked).Subscribe(b => ToggleOneClickHandler(b, kModelSaberProtocol));
            this.WhenAnyValue(x => x.PlaylistOneClickCheckBoxChecked).Subscribe(b => ToggleOneClickHandler(b, kPlaylistProtocol));
            IObservable<string> validatedInstallDirObservable = this.WhenAnyValue(x => x.InstallDir).Where(installDirValidator.ValidateInstallDir)!;
            validatedInstallDirObservable.BindTo(appSettings, x => x.Value.InstallDir);
            validatedInstallDirObservable.Select(installDirValidator.DetectVRPlatform).BindTo(appSettings, x => x.Value.VRPlatform);
            validatedInstallDirObservable.Select(_ => true).ToProperty(this, nameof(HasValidatedInstallDir), out _hasValidatedInstallDir);
            IObservable<string?> themesDirObservable = this.WhenAnyValue(x => x.ThemesDir).Where(x => !string.IsNullOrEmpty(x));
            themesDirObservable.BindTo(appSettings, x => x.Value.ThemesDir);
            themesDirObservable.Select(_ => true).ToProperty(this, nameof(OpenThemesDirButtonActive), out _openThemesDirButtonActive);
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallModLoaderCommand { get; }

        public ReactiveCommand<Unit, Unit> UninstallAllModsCommand { get; }

        public bool HasValidatedInstallDir => _hasValidatedInstallDir.Value;

        public bool OpenThemesDirButtonActive => _openThemesDirButtonActive.Value;

        private string? _installDir;
        public string? InstallDir
        {
            get => _installDir;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _themesDir;
            set => this.RaiseAndSetIfChanged(ref _themesDir, value);
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

        public async Task InstallPlaylistsAsync(string path) => await Task.Run(() => _playlistInstaller.InstallPlaylistAsync(path));

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }
    }
}