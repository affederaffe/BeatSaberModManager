using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using ReactiveUI;


namespace BeatSaberModManager.Views.Windows
{
    /// <summary>
    /// Dialog that asks the user to manually select the game's installation directory.
    /// </summary>
    public partial class InstallFolderDialogWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFolderDialogWindow"/> class.
        /// </summary>
        public InstallFolderDialogWindow()
        {
            InitializeComponent();
            ExtendClientAreaToDecorationsHint = !OperatingSystem.IsLinux();
            Margin = ExtendClientAreaToDecorationsHint ? WindowDecorationMargin : new Thickness();
            ContinueButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()))
                .Where(static x => x.Count > 0)
                .Select(static x => x[0].Path.LocalPath)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Close);
        }

        private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null!);
    }
}
