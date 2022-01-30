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
            _beatSaverOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(Constants.BeatSaverProtocol);
            _modelSaberOneClickCheckboxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(Constants.ModelSaberProtocol);
            _playlistOneClickCheckBoxChecked = protocolHandlerRegistrar.IsProtocolHandlerRegistered(Constants.PlaylistProtocol);
            AppSettings = appSettings;
            OpenInstallDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(appSettings.Value.InstallDir!), appSettings.Value.WhenAnyValue(static x => x.InstallDir).Select(Directory.Exists));
            OpenThemesDirCommand = ReactiveCommand.Create(() => PlatformUtils.OpenUri(appSettings.Value.ThemesDir!), appSettings.Value.WhenAnyValue(static x => x.ThemesDir).Select(Directory.Exists));
            this.WhenAnyValue(static x => x.BeatSaverOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, Constants.BeatSaverProtocol));
            this.WhenAnyValue(static x => x.ModelSaberOneClickCheckboxChecked).Subscribe(x => ToggleOneClickHandler(x, Constants.ModelSaberProtocol));
            this.WhenAnyValue(static x => x.PlaylistOneClickCheckBoxChecked).Subscribe(x => ToggleOneClickHandler(x, Constants.PlaylistProtocol));
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