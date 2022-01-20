using System;
using System.Reactive.Linq;

using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Converters;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Windows
{
    public partial class AssetInstallWindow : ReactiveWindow<AssetInstallWindowViewModel>
    {
        public AssetInstallWindow() { }

        [ActivatorUtilitiesConstructor]
        public AssetInstallWindow(AssetInstallWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            LocalizedStatusConverter converter = new(this);
            ViewModel.StatusProgress.ProgressInfo
                .Select(converter.Convert)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ViewModel.Log.Insert(0, x));
            ViewModel.InstallCommand.Execute()
                .Delay(TimeSpan.FromMilliseconds(2000))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Close());
        }
    }
}