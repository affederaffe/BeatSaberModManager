using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using ReactiveUI;


namespace BeatSaberModManager.Views.Windows
{
    public partial class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
            ContinueButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Close);
        }

        public void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);
    }
}