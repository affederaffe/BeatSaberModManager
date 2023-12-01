using System;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Labs.Controls;

using BeatSaberModManager.Views.Dialogs;

using LibDepotDownloader;


namespace BeatSaberModManager.Views.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class DialogSteamAuthenticator(Lazy<Window> window) : ISteamAuthenticator
    {

        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync() => Task.FromResult(true);

        /// <inheritdoc />
        public async ValueTask DisplayQrCode(string challengeUrl)
        {
            SteamAuthenticationQrCodeDialog dialog = new() { QrCode = { Data = challengeUrl } };
            await dialog.Dialog.ShowAsync(window.Value).ConfigureAwait(true);
        }

        /// <inheritdoc />
        public async ValueTask<(string? Username, string? Password, bool RememberLogin)> ProvideLoginDetailsAsync()
        {
            SteamAuthenticationLoginDetailsDialog dialog = new();
            ContentDialogResult result = await dialog.Dialog.ShowAsync(window.Value).ConfigureAwait(true);
            return result switch
            {
                ContentDialogResult.Primary => (dialog.UsernameTextBox.Text, dialog.PasswordTextBox.Text, dialog.RememberPasswordCheckBox.IsChecked.GetValueOrDefault()),
                ContentDialogResult.None or ContentDialogResult.Secondary => (null, null, false),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
