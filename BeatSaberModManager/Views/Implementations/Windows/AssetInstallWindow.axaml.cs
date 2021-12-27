using System;
using System.Reactive.Linq;

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
            ViewModel.WhenAnyValue(x => x.AssetName)
                .WhereNotNull()
                .Select(x => $"{installText} {x}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ViewModel.Log.Insert(0, x));
            ViewModel.InstallCommand.Execute()
                .Delay(TimeSpan.FromMilliseconds(2000))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Close());
        }
    }
}