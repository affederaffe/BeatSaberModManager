using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using LibDepotDownloader;

using ReactiveUI;

using SteamKit2.Authentication;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SteamAuthenticationViewModel : ViewModelBase, ISteamAuthenticator, IDisposable
    {
        private readonly Steam3Session _session;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// 
        /// </summary>
        public SteamAuthenticationViewModel(Steam3Session session)
        {
            ArgumentNullException.ThrowIfNull(session);
            _session = session;
            IObservable<bool> canLogin = this.WhenAnyValue(static x => x.Username, static x => x.Password)
                .Select(static x => !string.IsNullOrWhiteSpace(x.Item1) && string.IsNullOrWhiteSpace(x.Item2));
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
            CancelCommand = ReactiveCommand.Create(_cancellationTokenSource.Cancel);
        }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, AuthPollResult> LoginCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public string? LoginChallenge
        {
            get => _loginChallenge;
            set => this.RaiseAndSetIfChanged(ref _loginChallenge, value);
        }

        private string? _loginChallenge;

        /// <summary>
        /// 
        /// </summary>
        public string? Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        private string? _username;

        /// <summary>
        /// 
        /// </summary>
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string? _password;

        /// <summary>
        /// 
        /// </summary>
        public bool RememberPassword
        {
            get => _rememberPassword;
            set => this.RaiseAndSetIfChanged(ref _rememberPassword, value);
        }

        private bool _rememberPassword;

        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync() => Task.FromResult(true);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<AuthPollResult?> AuthenticateAsync()
        {
            IObservable<QrAuthSession> qrAuthSessionObservable = Observable.FromAsync(() => _session.SteamClient.Authentication.BeginAuthSessionViaQRAsync(new AuthSessionDetails { Authenticator = this }));
            qrAuthSessionObservable.SelectMany(static x => Observable
                    .FromEvent(handler => x.ChallengeURLChanged += handler, handler => x.ChallengeURLChanged -= handler)
                    .Select(_ => x.ChallengeURL)
                    .StartWith(x.ChallengeURL))
                .Subscribe(challengeUrl => LoginChallenge = challengeUrl);
            return await qrAuthSessionObservable.SelectMany(x => x.PollingWaitForResultAsync(_cancellationTokenSource.Token))
                .Merge(LoginCommand)
                .FirstAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }

        private async Task<AuthPollResult> LoginAsync()
        {
            AuthSessionDetails details = new()
            {
                Authenticator = this,
                Username = Username,
                Password = Password,
                IsPersistentSession = RememberPassword
            };

            CredentialsAuthSession authSession = await _session.SteamClient.Authentication.BeginAuthSessionViaCredentialsAsync(details).ConfigureAwait(false);
            return await authSession.PollingWaitForResultAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        }
    }
}
