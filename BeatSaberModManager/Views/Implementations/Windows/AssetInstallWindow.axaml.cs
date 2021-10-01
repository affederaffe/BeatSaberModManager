using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class AssetInstallWindow : ReactiveWindow<AssetInstallWindowViewModel>
    {
        private readonly Uri _uri = null!;

        public AssetInstallWindow() { }

        [ActivatorUtilitiesConstructor]
        public AssetInstallWindow(AssetInstallWindowViewModel assetInstallWindowViewModel, string url)
        {
            InitializeComponent();
            _uri = new Uri(url);
            ViewModel = assetInstallWindowViewModel;
            string? installText = this.FindResource("Status:Installing") as string;
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