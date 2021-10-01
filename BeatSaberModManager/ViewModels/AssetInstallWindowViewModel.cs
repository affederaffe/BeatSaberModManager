using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class AssetInstallWindowViewModel : ReactiveObject
    {
        private readonly IEnumerable<IAssetProvider> _assetProviders;
        private readonly IStatusProgress _progress;
        private readonly ObservableAsPropertyHelper<double> _progressValue;
        private readonly ObservableAsPropertyHelper<string?> _assetName;

        public AssetInstallWindowViewModel(IEnumerable<IAssetProvider> assetProviders, IStatusProgress progress)
        {
            _assetProviders = assetProviders;
            _progress = progress;
            StatusProgress statusProgress = (StatusProgress)progress;
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
            statusProgress.StatusText.ToProperty(this, nameof(AssetName), out _assetName);
        }

        public async Task InstallAsset(Uri uri)
        {
            IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == uri.Scheme);
            if (assetProvider is null) return;
            IsSuccess = await assetProvider.InstallAssetAsync(uri, _progress);
            IsFailed = !IsSuccess;
            IsInstalling = false;
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