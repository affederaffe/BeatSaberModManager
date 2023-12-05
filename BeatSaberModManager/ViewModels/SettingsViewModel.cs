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
    public sealed class SettingsViewModel : ViewModelBase, IDisposable
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;
        private readonly ObservableAsPropertyHelper<string?> _installDir;
        private readonly DirectoryExistsObservable _installDirExistsObservable;

        private const string BeatSaverScheme = "beatsaver";
        private const string ModelSaberScheme = "modelsaber";
        private const string BSPlaylistScheme = "bsplaylist";

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(ISettings<AppSettings> appSettings, IGameInstallLocator gameInstallLocator, IProtocolHandlerRegistrar protocolHandlerRegistrar)
        {
            ArgumentNullException.ThrowIfNull(appSettings);
            ArgumentNullException.ThrowIfNull(protocolHandlerRegistrar);
            _appSettings = appSettings;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(BeatSaverScheme);
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(ModelSaberScheme);
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(BSPlaylistScheme);
            PickInstallDirInteraction = new Interaction<Unit, string?>();
            _installDirExistsObservable = new DirectoryExistsObservable { Path = appSettings.Value.InstallDir };
            IObservable<IGameVersion?> gameVersionObservable = _installDirExistsObservable.SelectMany(_ => gameInstallLocator.DetectLocalInstallTypeAsync(_installDirExistsObservable.Path!));
            gameVersionObservable.FirstAsync().Subscribe(x => GameVersion = x);
            IsGameVersionValidObservable = gameVersionObservable.Select(static gameVersion => gameVersion is not null);
            ValidatedGameVersionObservable = gameVersionObservable.WhereNotNull();
            ValidatedGameVersionObservable.Select(static gameVersion => gameVersion.InstallDir)
                .ToProperty(this, nameof(InstallDir), out _installDir);
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(new Uri(_installDirExistsObservable.Path!)), _installDirExistsObservable.ObserveOn(RxApp.MainThreadScheduler));
            PickGameVersionCommand = ReactiveCommand.CreateFromObservable(() => PickInstallDirInteraction.Handle(Unit.Default)
                .WhereNotNull()
                .SelectMany(gameInstallLocator.DetectLocalInstallTypeAsync)
                .WhereNotNull());
            PickGameVersionCommand.Subscribe(x => GameVersion = x);
            this.WhenAnyValue(static x => x.GameVersion).WhereNotNull().Subscribe(x => _installDirExistsObservable.Path = x.InstallDir);
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, BeatSaverScheme));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, ModelSaberScheme));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, BSPlaylistScheme));
        }

        /// <summary>
        /// Signals when a valid installation directory is provided.
        /// </summary>
        public IObservable<bool> IsGameVersionValidObservable { get; }

        /// <summary>
        /// Signals when a valid installation directory is provided.
        /// </summary>
        public IObservable<IGameVersion> ValidatedGameVersionObservable { get; }

        /// <summary>
        /// Opens the <see cref="GameVersion"/> in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenInstallDirCommand { get; }

        /// <summary>
        /// Select a new installation directory.
        /// </summary>
        public ReactiveCommand<Unit, IGameVersion> PickGameVersionCommand { get; }

        /// <summary>
        /// Ask the user to pick an installation directory.
        /// </summary>
        public Interaction<Unit, string?> PickInstallDirInteraction { get; }

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
        /// The game's installation.
        /// </summary>
        public IGameVersion? GameVersion
        {
            get => _gameVersion;
            set
            {
                _appSettings.Value.InstallDir = this.RaiseAndSetIfChanged(ref _gameVersion, value)?.InstallDir;
                _ = _appSettings.SaveAsync();
            }
        }

        private IGameVersion? _gameVersion;

        /// <summary>
        /// The game's installation directory.
        /// </summary>
        public string? InstallDir => _installDir.Value;

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
            if (active)
                _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else
                _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _installDirExistsObservable.Dispose();
            OpenInstallDirCommand.Dispose();
            PickGameVersionCommand.Dispose();
        }
    }
}
