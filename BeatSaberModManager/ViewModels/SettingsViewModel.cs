using System;
using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Observables;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Pages.SettingsPage"/>.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IProtocolHandlerRegistrar protocolHandlerRegistrar)
        {
            _appSettings = appSettings;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("beatsaver");
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("modelsaber");
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("bsplaylist");
            DirectoryExistsObservable installDirExistsObservable = new();
            IsInstallDirValidObservable = installDirExistsObservable.Select(_ => installDirValidator.ValidateInstallDir(installDirExistsObservable.Path));
            ValidatedInstallDirObservable = IsInstallDirValidObservable.Where(static x => x).Select(_ => installDirExistsObservable.Path!);
            this.WhenAnyValue(static x => x.InstallDir).Subscribe(x => installDirExistsObservable.Path = x);
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(installDirExistsObservable.Path!), installDirExistsObservable.ObserveOn(RxApp.MainThreadScheduler));
            DirectoryExistsObservable themesDirExistsObservable = new();
            ValidatedThemesDirObservable = themesDirExistsObservable.Where(static x => x).Select(_ => themesDirExistsObservable.Path!);
            this.WhenAnyValue(static x => x.ThemesDir).Subscribe(x => themesDirExistsObservable.Path = x);
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(themesDirExistsObservable.Path!), themesDirExistsObservable.ObserveOn(RxApp.MainThreadScheduler));
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, "beatsaver"));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, "modelsaber"));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, "bsplaylist"));
        }

        /// <summary>
        /// Signals when a valid installation directory is provided.
        /// </summary>
        public IObservable<bool> IsInstallDirValidObservable { get; }

        /// <summary>
        /// Signals when a valid installation directory is provided.
        /// </summary>
        public IObservable<string> ValidatedInstallDirObservable { get; }

        /// <summary>
        /// Signals when a valid themes directory is provided.
        /// </summary>
        public IObservable<string> ValidatedThemesDirObservable { get; }

        /// <summary>
        /// Opens the <see cref="InstallDir"/> in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenInstallDirCommand { get; }

        /// <summary>
        /// Opens the <see cref="ThemesDir"/> in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenThemesDirCommand { get; }

        /// <inheritdoc cref="AppSettings.SaveSelectedMods" />
        public bool SaveSelectedMods
        {
            get => _appSettings.Value.SaveSelectedMods;
            set => _appSettings.Value.SaveSelectedMods = value;
        }

        /// <inheritdoc cref="AppSettings.ForceReinstallMods" />
        public bool ForceReinstallMods
        {
            get => _appSettings.Value.ForceReinstallMods;
            set => _appSettings.Value.ForceReinstallMods = value;
        }

        /// <inheritdoc cref="AppSettings.CloseOneClickWindow" />
        public bool CloseOneClickWindow
        {
            get => _appSettings.Value.CloseOneClickWindow;
            set => _appSettings.Value.CloseOneClickWindow = value;
        }

        /// <summary>
        /// Checks or unchecks the checkbox control.
        /// </summary>
        public bool BeatSaverOneClickCheckboxChecked
        {
            get => _beatSaverOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _beatSaverOneClickCheckboxChecked, value);
        }

        private bool _beatSaverOneClickCheckboxChecked;

        /// <summary>
        /// Checks or unchecks the checkbox control.
        /// </summary>
        public bool ModelSaberOneClickCheckboxChecked
        {
            get => _modelSaberOneClickCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref _modelSaberOneClickCheckboxChecked, value);
        }

        private bool _modelSaberOneClickCheckboxChecked;

        /// <summary>
        /// Checks or unchecks the checkbox control.
        /// </summary>
        public bool PlaylistOneClickCheckBoxChecked
        {
            get => _playlistOneClickCheckBoxChecked;
            set => this.RaiseAndSetIfChanged(ref _playlistOneClickCheckBoxChecked, value);
        }

        private bool _playlistOneClickCheckBoxChecked;

        /// <summary>
        /// The game's installation directory.
        /// </summary>
        public string? InstallDir
        {
            get => _installDir ??= _appSettings.Value.InstallDir;
            set => _appSettings.Value.InstallDir = this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _installDir;

        /// <summary>
        /// A directory containing additional Themes.
        /// </summary>
        public string? ThemesDir
        {
            get => _themesDir ??= _appSettings.Value.ThemesDir;
            set => _appSettings.Value.ThemesDir = this.RaiseAndSetIfChanged(ref _themesDir, value);
        }

        private string? _themesDir;

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }
    }
}
