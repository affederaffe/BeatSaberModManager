using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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

        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// 
        /// </summary>
        public SteamAuthenticationViewModel(Steam3Session session)
        {
            ArgumentNullException.ThrowIfNull(session);
            _session = session;
            IObservable<bool> canLogin = this.WhenAnyValue(static x => x.Username, static x => x.Password)
                .Select(static x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2));
            LoginCommand = ReactiveCommand.CreateFromTask(LoginWithCredentialsAsync, canLogin);
            CancelCommand = ReactiveCommand.Create(() => _cancellationTokenSource?.Cancel());
            IObservable<bool> canSubmitSteamGuardCode = this.WhenAnyValue(static x => x.SteamGuardCode)
                .Select(static code => code is not null && code.Length == 5);
            SubmitSteamGuardCodeCommand = ReactiveCommand.Create(() => SteamGuardCode!, canSubmitSteamGuardCode);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public Interaction<Unit, AuthPollResult?> AuthenticateSteamInteraction { get; } = new(RxApp.MainThreadScheduler);

        /// <summary>
        /// 
        /// </summary>
        public Interaction<bool, string> GetSteamGuardCodeInteraction { get; } = new(RxApp.MainThreadScheduler);

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, AuthPollResult?> LoginCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, string> SubmitSteamGuardCodeCommand { get; }

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

        /// <summary>
        /// 
        /// </summary>
        public string? SteamGuardCode
        {
            get => _steamGuardCode;
            set => this.RaiseAndSetIfChanged(ref _steamGuardCode, value);
        }

        private string? _steamGuardCode;

        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) => GetSteamGuardCodeInteraction.Handle(previousCodeWasIncorrect).ToTask();

        /// <inheritdoc />
        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect) => throw new System.NotImplementedException();

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync() => Task.FromResult(false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<AuthPollResult?> AuthenticateAsync() => AuthenticateSteamInteraction.Handle(Unit.Default).ToTask();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<AuthPollResult?> StartAuthenticationAsync()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            return Observable.StartAsync(LoginWithQrCodeAsync)
                .Merge(LoginCommand)
                .FirstAsync()
                .ToTask();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        private async Task<AuthPollResult?> LoginWithCredentialsAsync()
        {
            AuthSessionDetails details = new()
            {
                Authenticator = this,
                Username = Username,
                Password = Password,
                IsPersistentSession = RememberPassword
            };

            CredentialsAuthSession authSession = await _session.SteamClient.Authentication.BeginAuthSessionViaCredentialsAsync(details).ConfigureAwait(false);

            try
            {
                AuthPollResult result = await authSession.PollingWaitForResultAsync(_cancellationTokenSource!.Token).ConfigureAwait(false);
                await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
                return result;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private async Task<AuthPollResult?> LoginWithQrCodeAsync()
        {
            AuthSessionDetails details = new()
            {
                Authenticator = this
            };

            QrAuthSession authSession = await _session.SteamClient.Authentication.BeginAuthSessionViaQRAsync(details).ConfigureAwait(false);
            LoginChallenge = authSession.ChallengeURL;
            authSession.ChallengeURLChanged += () => LoginChallenge = authSession.ChallengeURL;

            try
            {
                AuthPollResult result = await authSession.PollingWaitForResultAsync(_cancellationTokenSource!.Token).ConfigureAwait(false);
                await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
                return result;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
    }
}
