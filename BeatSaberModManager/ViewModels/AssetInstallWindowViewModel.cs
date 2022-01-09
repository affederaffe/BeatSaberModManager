using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    public class AssetInstallWindowViewModel : ViewModelBase
    {
        private readonly Uri _uri;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IEnumerable<IAssetProvider> _assetProviders;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        public AssetInstallWindowViewModel(Uri uri, IOptions<AppSettings> appSettings, IInstallDirValidator installDirValidator, IStatusProgress progress, IEnumerable<IAssetProvider> assetProviders)
        {
            _uri = uri;
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _assetProviders = assetProviders;
            StatusProgress = (StatusProgress)progress;
            InstallCommand = ReactiveCommand.CreateFromTask(InstallAssetAsync);
            InstallCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InstallCommand.ToProperty(this, nameof(IsSuccess), out _isSuccess);
            InstallCommand.CombineLatest(InstallCommand.IsExecuting)
                .Select(x => !x.First && !x.Second)
                .ToProperty(this, nameof(IsFailed), out _isFailed);
            StatusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
        }

        public ReactiveCommand<Unit, bool> InstallCommand { get; }

        public ObservableCollection<string> Log { get; } = new();

        public StatusProgress StatusProgress { get; }

        public bool IsExecuting => _isExecuting.Value;

        public bool IsSuccess => _isSuccess.Value;

        public bool IsFailed => _isFailed.Value;

        public double ProgressValue => _progressValue.Value;

        private async Task<bool> InstallAssetAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return false;
            IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == _uri.Scheme);
            return assetProvider is not null && await assetProvider.InstallAssetAsync(_appSettings.Value.InstallDir.Value!, _uri, StatusProgress).ConfigureAwait(false);
        }
    }
}