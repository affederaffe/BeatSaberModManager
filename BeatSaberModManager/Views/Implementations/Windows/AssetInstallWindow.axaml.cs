using System;
using System.Reactive;
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
        public AssetInstallWindow() { }

        [ActivatorUtilitiesConstructor]
        public AssetInstallWindow(AssetInstallWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            string? installText = this.FindResource("Status:Installing") as string;
            ViewModel.WhenAnyValue(x => x!.AssetName)
                .WhereNotNull()
                .Select(x => $"{installText} {x}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ViewModel.Log.Insert(0, x));
            ReactiveCommand<AssetInstallWindowViewModel, Unit> installAssetCommand = ReactiveCommand.CreateFromTask<AssetInstallWindowViewModel>(InstallAsset);
            this.WhenAnyValue(x => x.ViewModel).InvokeCommand(installAssetCommand!);
        }

        private async Task InstallAsset(AssetInstallWindowViewModel viewModel)
        {
            await viewModel.InstallAsset();
            await Task.Delay(2000);
            Close();
        }
    }
}