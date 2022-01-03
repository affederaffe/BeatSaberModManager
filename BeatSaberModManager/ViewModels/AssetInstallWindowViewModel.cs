using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;

        public AssetInstallWindowViewModel(Uri uri, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IStatusProgress progress, IEnumerable<IAssetProvider> assetProviders)
        {
            _uri = uri;
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _progress = progress;
            _assetProviders = assetProviders;
            StatusProgress = (StatusProgress)progress;
            InstallCommand = ReactiveCommand.CreateFromTask(InstallAssetAsync);
            InstallCommand.ToProperty(this, nameof(IsSuccess), out _isSuccess);
            InstallCommand.CombineLatest(InstallCommand.IsExecuting)
                .Select(x => !x.First && !x.Second)
                .ToProperty(this, nameof(IsFailed), out _isFailed);
        }

        public ReactiveCommand<Unit, bool> InstallCommand { get; }

        public ObservableCollection<string> Log { get; } = new();

        public StatusProgress StatusProgress { get; }

        public bool IsSuccess => _isSuccess.Value;

        public bool IsFailed => _isFailed.Value;

        private async Task<bool> InstallAssetAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return false;
            IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == _uri.Scheme);
            return assetProvider is not null && await assetProvider.InstallAssetAsync(_appSettings.Value.InstallDir.Value!, _uri, _progress).ConfigureAwait(false);
        }
    }
}