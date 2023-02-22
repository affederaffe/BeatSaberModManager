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

        private const string BeatSaverScheme = "beatsaver";
        private const string ModelSaberScheme = "modelsaber";
        private const string BSPlaylistScheme = "bsplaylist";

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IProtocolHandlerRegistrar protocolHandlerRegistrar)
        {
            _appSettings = appSettings;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(BeatSaverScheme);
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(ModelSaberScheme);
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(BSPlaylistScheme);
            DirectoryExistsObservable installDirExistsObservable = new();
            IsInstallDirValidObservable = installDirExistsObservable.Select(_ => installDirValidator.ValidateInstallDir(installDirExistsObservable.Path));
            ValidatedInstallDirObservable = IsInstallDirValidObservable.Where(static x => x).Select(_ => installDirExistsObservable.Path!);
            this.WhenAnyValue(static x => x.InstallDir).Subscribe(x => installDirExistsObservable.Path = x);
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(installDirExistsObservable.Path!), installDirExistsObservable.ObserveOn(RxApp.MainThreadScheduler));
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, BeatSaverScheme));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, ModelSaberScheme));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, BSPlaylistScheme));
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
        /// Opens the <see cref="InstallDir"/> in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenInstallDirCommand { get; }

        /// <inheritdoc cref="AppSettings.TabIndex" />
        public int TabIndex
        {
            get => _appSettings.Value.TabIndex;
            set => _appSettings.Value.TabIndex = value;
        }

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
        /// The game's installation directory.
        /// </summary>
        public string? InstallDir
        {
            get => _installDir ??= _appSettings.Value.InstallDir;
            set => _appSettings.Value.InstallDir = this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _installDir;

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

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }
    }
}
