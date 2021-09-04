using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views
{
    public partial class AssetInstallWindow : ReactiveWindow<AssetInstallWindowViewModel>
    {
        private readonly Uri _uri = null!;

        public AssetInstallWindow() { }

        [ActivatorUtilitiesConstructor]
        public AssetInstallWindow(AssetInstallWindowViewModel assetInstallWindowViewModel, Uri uri)
        {
            _uri = uri;
            InitializeComponent();
            ViewModel = assetInstallWindowViewModel;
            string installText = (string)this.FindResource("AssetInstallWindow:InstallText")!;
            ViewModel.WhenAnyValue(x => x!.AssetName)
                .WhereNotNull()
                .Select(x => $"{installText} {x}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ViewModel.Log.Insert(0, x));
            this.WhenActivated(_ => InstallAsset().ConfigureAwait(false));
        }

        private async Task InstallAsset()
        {
            await ViewModel!.InstallAsset(_uri);
            await Task.Delay(1500);
            Close();
        }
    }
}