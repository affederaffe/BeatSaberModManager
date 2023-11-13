using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Converters;

using ReactiveUI;


namespace BeatSaberModManager.Views.Windows
{
    /// <summary>
    /// Top-level view to display progress information when using the '--install' flag.
    /// </summary>
    public partial class AssetInstallWindow : ReactiveWindow<AssetInstallWindowViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public AssetInstallWindow() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetInstallWindow"/> class.
        /// </summary>
        public AssetInstallWindow(AssetInstallWindowViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            InitializeComponent();
            ViewModel = viewModel;
            ExtendClientAreaToDecorationsHint = !OperatingSystem.IsLinux();
            Margin = ExtendClientAreaToDecorationsHint ? WindowDecorationMargin : new Thickness();
            viewModel.ProgressInfoObservable
                .Select(x => $"{(this.TryFindResource(StatusTypeEnumConverter.Convert(x.StatusType), out object? value) ? value : null)}: {x.Text}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ViewModel.Log.Insert(0, x));
            IObservable<bool> executeObservable = ViewModel.InstallCommand.Execute();
            if (viewModel.CloseOneClickWindow)
                executeObservable.Delay(TimeSpan.FromMilliseconds(2000)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => Close());
            else
                executeObservable.Subscribe();
        }
    }
}
