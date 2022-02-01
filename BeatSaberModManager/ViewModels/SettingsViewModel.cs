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

        public SettingsViewModel(IOptions<AppSettings> appSettings, IProtocolHandlerRegistrar protocolHandlerRegistrar)
        {
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("beatsaver");
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("modelsaber");
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered("bsplaylist");
            AppSettings = appSettings;
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(appSettings.Value.InstallDir!), appSettings.Value.WhenAnyValue(static x => x.InstallDir).Select(Directory.Exists));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.TryOpenUri(appSettings.Value.ThemesDir!), appSettings.Value.WhenAnyValue(static x => x.ThemesDir).Select(Directory.Exists));
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, "beatsaver"));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, "modelsaber"));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, "bsplaylist"));
        }

        public ReactiveCommand<Unit, bool> OpenInstallDirCommand { get; }

        public ReactiveCommand<Unit, bool> OpenThemesDirCommand { get; }

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