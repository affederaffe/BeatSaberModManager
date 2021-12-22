using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class AssetInstallWindowViewModel : ViewModelBase
    {
        private readonly Uri _uri;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IStatusProgress _progress;
        private readonly IEnumerable<IAssetProvider> _assetProviders;
        private readonly ObservableAsPropertyHelper<double> _progressValue;
        private readonly ObservableAsPropertyHelper<string?> _assetName;

        public AssetInstallWindowViewModel(Uri uri, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IStatusProgress progress, IEnumerable<IAssetProvider> assetProviders)
        {
            _uri = uri;
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _progress = progress;
            _assetProviders = assetProviders;
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
            statusProgress.StatusText.ToProperty(this, nameof(AssetName), out _assetName);
        }

        public async Task InstallAssetAsync()
        {
            if (_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value))
            {
                IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == _uri.Scheme);
                if (assetProvider is not null) IsSuccess = await assetProvider.InstallAssetAsync(_appSettings.Value.InstallDir.Value!, _uri, _progress).ConfigureAwait(false);
                IsFailed = !IsSuccess;
                IsInstalling = false;
            }
            else
            {
                IsFailed = true;
                IsInstalling = false;
            }
        }

        public ObservableCollection<string> Log { get; } = new();

        public double ProgressValue => _progressValue.Value;

        public string? AssetName => _assetName.Value;

        private bool _isInstalling = true;
        public bool IsInstalling
        {
            get => _isInstalling;
            set => this.RaiseAndSetIfChanged(ref _isInstalling, value);
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set => this.RaiseAndSetIfChanged(ref _isSuccess, value);
        }

        private bool _isFailed;
        public bool IsFailed
        {
            get => _isFailed;
            set => this.RaiseAndSetIfChanged(ref _isFailed, value);
        }
    }
}