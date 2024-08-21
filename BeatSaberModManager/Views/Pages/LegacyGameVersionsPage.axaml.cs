using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Labs.Controls;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Dialogs;

using SteamKit2.Authentication;


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
            viewModel.SteamAuthenticationViewModel.AuthenticateSteamInteraction.RegisterHandler(async context =>
            {
                Window window = (VisualRoot as Window)!;
                SteamLoginView steamLoginView = new() { DataContext = viewModel.SteamAuthenticationViewModel };
                ContentDialog dialog = new()
                {
                    Content = steamLoginView,
                    Title = this.FindResource("SteamLoginView:Title") as string,
                    PrimaryButtonText = this.FindResource("SteamLoginView:LoginButton") as string,
                    PrimaryButtonCommand = viewModel.SteamAuthenticationViewModel.LoginCommand,
                    SecondaryButtonText = this.FindResource("SteamLoginView:CancelButton") as string,
                    SecondaryButtonCommand = viewModel.SteamAuthenticationViewModel.CancelCommand
                };

                Task<ContentDialogResult> task = dialog.ShowAsync(window);
                AuthPollResult? authenticationResult = await viewModel.SteamAuthenticationViewModel.StartAuthenticationAsync().ConfigureAwait(true);
                context.SetOutput(authenticationResult);
                dialog.Hide();
                await task.ConfigureAwait(false);
            });

            viewModel.SteamAuthenticationViewModel.GetSteamGuardCodeInteraction.RegisterHandler(async context =>
            {
                Window window = (VisualRoot as Window)!;
                SteamGuardCodeView steamGuardCodeView = new() { DataContext = viewModel.SteamAuthenticationViewModel };
                ContentDialog dialog = new()
                {
                    Content = steamGuardCodeView,
                    Title = this.FindResource("SteamGuardView:Title") as string,
                    PrimaryButtonText = this.FindResource("SteamGuardView:SubmitButton") as string,
                    PrimaryButtonCommand = viewModel.SteamAuthenticationViewModel.LoginCommand,
                    SecondaryButtonText = this.FindResource("SteamGuardView:CancelButton") as string,
                    SecondaryButtonCommand = viewModel.SteamAuthenticationViewModel.CancelCommand
                };

                Task<ContentDialogResult> task = dialog.ShowAsync(window);
                string code = await viewModel.SteamAuthenticationViewModel.SubmitSteamGuardCodeCommand.ToTask().ConfigureAwait(true);
                context.SetOutput(code);
                dialog.Hide();
                await task.ConfigureAwait(false);
            });
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
