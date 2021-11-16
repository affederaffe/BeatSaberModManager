using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow() { }

        [ActivatorUtilitiesConstructor]
        public MainWindow(MainWindowViewModel mainWindowViewModel, IEnumerable<IPage> pages)
        {
            InitializeComponent();
            ViewModel = mainWindowViewModel;
            int i = 0;
            IPage[] pageItems = pages.ToArray();
            foreach (TabItem tabItem in Pages.Items)
                tabItem.Content = pageItems[i++];
            ViewModel.WhenAnyValue(x => x.ProgressBarStatusType)
                .Select(GetLocalizedStatus)
                .BindTo(ViewModel, x => x.ProgressBarStatusText);
        }

        private string? GetLocalizedStatus(ProgressBarStatusType statusType) => this.FindResource(statusType switch
        {
            ProgressBarStatusType.None => string.Empty,
            ProgressBarStatusType.Installing => "Status:Installing",
            ProgressBarStatusType.Uninstalling => "Status:Uninstalling",
            _ => string.Empty
        }) as string;
    }
}