using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Interfaces;

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
            bool result = await assetProvider.InstallAssetAsync(uri, _progress);
            ProgressRingVisible = false;
            ResultLabelText = result ? "✔" : "✘";
        }

        public ObservableCollection<string> Log { get; } = new();

        public double ProgressValue => _progressValue.Value;

        public string? AssetName => _assetName.Value;

        private bool _progressRingVisible = true;
        public bool ProgressRingVisible
        {
            get => _progressRingVisible;
            set => this.RaiseAndSetIfChanged(ref _progressRingVisible, value);
        }

        private string? _resultLabelText;
        public string? ResultLabelText
        {
            get => _resultLabelText;
            set => this.RaiseAndSetIfChanged(ref _resultLabelText, value);
        }
    }
}