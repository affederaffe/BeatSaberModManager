using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
        private readonly ObservableAsPropertyHelper<string> _assetName;

        public AssetInstallWindowViewModel(IEnumerable<IAssetProvider> assetProviders, StatusProgress progress)
        {
            _assetProviders = assetProviders;
            _progress = progress;
            Observable.FromEventPattern<(double, string)>(handler => progress.ProgressChanged += handler, handler => progress.ProgressChanged -= handler)
                .Select(x => x.EventArgs.Item2)
                .ToProperty(this, nameof(AssetName), out _assetName);
        }

        public async Task InstallAsset(string strUri)
        {
            Uri uri = new(strUri);
            IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == uri.Scheme);
            if (assetProvider is null) return;
            bool result = await assetProvider.InstallAssetAsync(uri, _progress);
            ProgressRingActive = false;
            ResultLabelText = result ? "✔" : "✘";
        }

        public string AssetName => _assetName.Value;

        private bool _progressRingActive = true;
        public bool ProgressRingActive
        {
            get => _progressRingActive;
            set => this.RaiseAndSetIfChanged(ref _progressRingActive, value);
        }

        private string? _resultLabelText;
        public string? ResultLabelText
        {
            get => _resultLabelText;
            set => this.RaiseAndSetIfChanged(ref _resultLabelText, value);
        }
    }
}