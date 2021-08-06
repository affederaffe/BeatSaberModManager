using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class AssetInstallWindowViewModel : ReactiveObject
    {
        private readonly IEnumerable<IAssetProvider> _assetProviders;

        public AssetInstallWindowViewModel(IEnumerable<IAssetProvider> assetProviders)
        {
            _assetProviders = assetProviders;
        }

        public async Task InstallAsset(string strUri)
        {
            Uri uri = new(strUri);
            IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == uri.Scheme);
            if (assetProvider is null) return;
            string decodedName = WebUtility.UrlDecode(uri.Segments.Last());
            AssetName = decodedName == "/" ? uri.Host : decodedName;
            bool result = await assetProvider.InstallAssetAsync(uri);
            ProgressRingActive = false;
            ResultLabelText = result ? "✔" : "✘";
        }

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

        private string? _assetName;
        public string? AssetName
        {
            get => _assetName;
            set => this.RaiseAndSetIfChanged(ref _assetName, value);
        }
    }
}