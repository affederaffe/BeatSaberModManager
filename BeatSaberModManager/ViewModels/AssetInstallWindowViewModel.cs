using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="BeatSaberModManager.Views.Windows.AssetInstallWindow"/>.
    /// </summary>
    public sealed class AssetInstallWindowViewModel : ViewModelBase
    {
        private readonly Uri _uri;
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IStatusProgress _progress;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IEnumerable<IAssetProvider> _assetProviders;
        private readonly ObservableAsPropertyHelper<bool> _isExecuting;
        private readonly ObservableAsPropertyHelper<bool> _isSuccess;
        private readonly ObservableAsPropertyHelper<bool> _isFailed;
        private readonly ObservableAsPropertyHelper<double> _progressValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetInstallWindowViewModel"/> class.
        /// </summary>
        public AssetInstallWindowViewModel(Uri uri, StatusProgress statusProgress, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, IEnumerable<IAssetProvider> assetProviders)
        {
            ArgumentNullException.ThrowIfNull(statusProgress);
            ArgumentNullException.ThrowIfNull(assetProviders);
            _uri = uri;
            _progress = statusProgress;
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _assetProviders = assetProviders;
            ProgressInfoObservable = statusProgress.ProgressInfo;
            Log = new ObservableCollection<string>();
            InstallCommand = ReactiveCommand.CreateFromTask(InstallAssetAsync);
            InstallCommand.IsExecuting.ToProperty(this, nameof(IsExecuting), out _isExecuting);
            InstallCommand.ToProperty(this, nameof(IsSuccess), out _isSuccess);
            InstallCommand.CombineLatest(InstallCommand.IsExecuting)
                .Select(static x => x is (false, false))
                .ToProperty(this, nameof(IsFailed), out _isFailed);
            statusProgress.ProgressValue.ToProperty(this, nameof(ProgressValue), out _progressValue);
        }

        /// <inheritdoc cref="StatusProgress.ProgressInfo" />
        public IObservable<ProgressInfo> ProgressInfoObservable { get; }

        /// <summary>
        /// Downloads and installs an asset.
        /// </summary>
        public ReactiveCommand<Unit, bool> InstallCommand { get; }

        /// <summary>
        /// Collection of log messages.
        /// </summary>
        public ObservableCollection<string> Log { get; }

        /// <inheritdoc cref="AppSettings.CloseOneClickWindow" />
        public bool CloseOneClickWindow => _appSettings.Value.CloseOneClickWindow;

        /// <summary>
        /// True if the operation is currently executing, false otherwise.
        /// </summary>
        public bool IsExecuting => _isExecuting.Value;

        /// <summary>
        /// True if the operation successfully ran to completion, false otherwise.
        /// </summary>
        public bool IsSuccess => _isSuccess.Value;

        /// <summary>
        /// True if the operation faulted, false otherwise.
        /// </summary>
        public bool IsFailed => _isFailed.Value;

        /// <summary>
        /// The current progress of the operation.
        /// </summary>
        public double ProgressValue => _progressValue.Value;

        private async Task<bool> InstallAssetAsync()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir))
                return false;
            IAssetProvider? assetProvider = _assetProviders.FirstOrDefault(x => x.Protocol == _uri.Scheme);
            return assetProvider is not null && await assetProvider.InstallAssetAsync(_appSettings.Value.InstallDir, _uri, _progress).ConfigureAwait(false);
        }
    }
}
