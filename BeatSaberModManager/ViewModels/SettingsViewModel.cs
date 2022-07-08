using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
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
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(ISettings<AppSettings> appSettings, IProtocolHandlerRegistrar protocolHandlerRegistrar)
        {
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("beatsaver");
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("modelsaber");
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("bsplaylist");
            AppSettings = appSettings;
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(appSettings.Value.InstallDir!),
                appSettings.Value.WhenAnyValue(static x => x.InstallDir).Select(Directory.Exists));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(appSettings.Value.ThemesDir!),
                appSettings.Value.WhenAnyValue(static x => x.ThemesDir).Select(Directory.Exists));
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, "beatsaver"));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, "modelsaber"));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, "bsplaylist"));
        }

        /// <summary>
        /// Opens the <see cref="BeatSaberModManager.Models.Implementations.Settings.AppSettings.InstallDir"/> in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenInstallDirCommand { get; }

        /// <summary>
        /// Opens the <see cref="BeatSaberModManager.Models.Implementations.Settings.AppSettings.ThemesDir"/> in the file explorer.
        /// </summary>
        public ReactiveCommand<Unit, bool> OpenThemesDirCommand { get; }

        /// <summary>
        /// Exposed for the view.
        /// </summary>
        public ISettings<AppSettings> AppSettings { get; }

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
