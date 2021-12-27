using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;


namespace BeatSaberModManager.Views.Implementations.Windows
{
    public partial class InstallFolderDialogWindow : Window
    {
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
            ContinueButton.GetObservable(Button.ClickEvent)
                .Select(_ => new OpenFolderDialog().ShowAsync(this))
                .Subscribe(Close);
        }

        public void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);
    }
}