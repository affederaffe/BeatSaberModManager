using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;

        private const string kBeatSaverProtocol = "beatsaver";
        private const string kModelSaberProtocol = "modelsaber";
        private const string kPlaylistProtocol = "bsplaylist";

        public SettingsViewModel(IOptions<AppSettings> appSettings, IInstallDirValidator installDirValidator, IProtocolHandlerRegistrar protocolHandlerRegistrar)
        {
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(kBeatSaverProtocol);
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(kModelSaberProtocol);
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(kPlaylistProtocol);
            AppSettings = appSettings;
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(appSettings.Value.InstallDir!), appSettings.Value.WhenAnyValue(static x => x.InstallDir).Select(installDirValidator.ValidateInstallDir));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(appSettings.Value.ThemesDir!), appSettings.Value.WhenAnyValue(static x => x.ThemesDir).Select(Directory.Exists));
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, kBeatSaverProtocol));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, kModelSaberProtocol));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, kPlaylistProtocol));
        }

        public ReactiveCommand<Unit, Unit> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenThemesDirCommand { get; }

        public IOptions<AppSettings> AppSettings { get; }

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

        private void ToggleOneClickHandler(bool active, string protocol)
        {
            if (active) _protocolHandlerRegistrar.RegisterProtocolHandler(protocol);
            else _protocolHandlerRegistrar.UnregisterProtocolHandler(protocol);
        }
    }
}