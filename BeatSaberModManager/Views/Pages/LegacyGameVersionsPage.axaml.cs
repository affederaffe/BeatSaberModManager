using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Labs.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Dialogs;


namespace BeatSaberModManager.Views.Pages
{
    /// <summary>
    /// TODO
    /// </summary>
    public partial class LegacyGameVersionsPage : ReactiveUserControl<LegacyGameVersionsViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public LegacyGameVersionsPage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyGameVersionsPage"/> class.
        /// </summary>
        public LegacyGameVersionsPage(LegacyGameVersionsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            InitializeComponent();
            ViewModel = viewModel;
            viewModel.SteamAuthenticationViewModel.AuthenticateInteraction.RegisterHandler(async context => context.SetOutput(await ShowAuthenticateSteamDialogAsync(context.Input).ConfigureAwait(false)));
            viewModel.SteamAuthenticationViewModel.GetDeviceCodeInteraction.RegisterHandler(async context => context.SetOutput(await ShowSteamGuardCodeDialogAsync().ConfigureAwait(false)));
            viewModel.SteamAuthenticationViewModel.DeviceConfirmationInteraction.RegisterHandler(async context => context.SetOutput(await ShowDeviceConfirmationDialogAsync(context.Input).ConfigureAwait(false)));
        }

        private async Task<string?> ShowSteamGuardCodeDialogAsync()
        {
            Window window = (VisualRoot as Window)!;
            SteamGuardCodeView steamGuardCodeView = new() { DataContext = ViewModel!.SteamAuthenticationViewModel };
            ContentDialog dialog = new()
            {
                Content = steamGuardCodeView,
                Title = this.FindResource("SteamGuardCodeView:Title") as string,
                PrimaryButtonText = this.FindResource("SteamGuardCodeView:SubmitButton") as string,
                PrimaryButtonCommand = ViewModel.SteamAuthenticationViewModel.SubmitSteamGuardCodeCommand,
                SecondaryButtonText = this.FindResource("SteamGuardCodeView:CancelButton") as string,
                SecondaryButtonCommand = ViewModel.SteamAuthenticationViewModel.CancelCommand
            };

            ContentDialogResult result = await dialog.ShowAsync(window).ConfigureAwait(true);
            return result == ContentDialogResult.Primary ? ViewModel.SteamAuthenticationViewModel.SteamGuardCode : null;
        }

        private async Task<Unit> ShowAuthenticateSteamDialogAsync(CancellationToken cancellationToken)
        {
            Window window = (VisualRoot as Window)!;
            SteamLoginView steamLoginView = new() { DataContext = ViewModel!.SteamAuthenticationViewModel };
            ContentDialog dialog = new()
            {
                Content = steamLoginView,
                Title = this.FindResource("SteamLoginView:Title") as string,
                PrimaryButtonText = this.FindResource("SteamLoginView:LoginButton") as string,
                PrimaryButtonCommand = ViewModel.SteamAuthenticationViewModel.LoginCommand,
                SecondaryButtonText = this.FindResource("SteamLoginView:CancelButton") as string,
                SecondaryButtonCommand = ViewModel.SteamAuthenticationViewModel.CancelCommand
            };

            using IDisposable subscription = cancellationToken.Register(() => Dispatcher.UIThread.Post(() => dialog.Hide()));
            await dialog.ShowAsync(window).ConfigureAwait(true);
            return Unit.Default;
        }

        private async Task<Unit> ShowDeviceConfirmationDialogAsync(CancellationToken cancellationToken)
        {
            Window window = (VisualRoot as Window)!;
            ContentDialog dialog = new()
            {
                Content = this.FindResource("SteamDeviceConfirmationView:ConfirmLogin") as string,
                Title = this.FindResource("SteamDeviceConfirmationView:Title") as string
            };

            using IDisposable subscription = cancellationToken.Register(() => Dispatcher.UIThread.Post(() => dialog.Hide()));
            await dialog.ShowAsync(window).ConfigureAwait(true);
            return Unit.Default;
        }

        // Hack that forces the selected item to become highlighted again after switching tabs
        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Avalonia.Controls.TabControl tabControl = (sender as Avalonia.Controls.TabControl)!;
            ListBox? listBox = tabControl.FindDescendantOfType<ListBox>();
            if (listBox is not null)
                listBox.SelectedItem = ViewModel!.SelectedGameVersion;
        }
    }
}
