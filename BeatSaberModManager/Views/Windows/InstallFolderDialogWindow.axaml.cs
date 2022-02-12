using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using JetBrains.Annotations;

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
            ContinueButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Close);
        }

        [UsedImplicitly]
        private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => Close(null);
    }
}