using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;

using LibDepotDownloader;

using ReactiveUI;

using SteamKit2.Authentication;


namespace BeatSaberModManager.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SteamAuthenticationViewModel : ViewModelBase, ILegacyGameVersionAuthenticator, ISteamAuthenticator, IDisposable
    {
        private readonly Steam3Session _session;

        private CancellationTokenSource? _credentialsAuthSessionCts;
        private CancellationTokenSource? _qrAuthSessionCts;
        private CancellationTokenSource? _deviceConfirmationCts;

        /// <summary>
        /// 
        /// </summary>
        public SteamAuthenticationViewModel(Steam3Session session)
        {
            ArgumentNullException.ThrowIfNull(session);
            _session = session;
            IObservable<bool> canLogin = this.WhenAnyValue(static x => x.Username, static x => x.Password)
                .Select(static x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2));
            LoginCommand = ReactiveCommand.Create(() => { _qrAuthSessionCts?.Cancel(); }, canLogin);
            CancelCommand = ReactiveCommand.Create(() => { _credentialsAuthSessionCts?.Cancel(); _qrAuthSessionCts?.Cancel(); });
            IObservable<bool> canSubmitSteamGuardCode = this.WhenAnyValue(static x => x.SteamGuardCode)
                .Select(static code => code is not null && code.Length == 5);
            SubmitSteamGuardCodeCommand = ReactiveCommand.Create(() => SteamGuardCode!, canSubmitSteamGuardCode);
        }

        /// <summary>
        /// 
        /// </summary>
        public Interaction<CancellationToken, Unit> AuthenticateInteraction { get; } = new(RxApp.MainThreadScheduler);

        /// <summary>
        /// 
        /// </summary>
        public Interaction<CancellationToken, Unit> DeviceConfirmationInteraction { get; } = new(RxApp.MainThreadScheduler);

        /// <summary>
        /// 
        /// </summary>
        public Interaction<Unit, string?> GetDeviceCodeInteraction { get; } = new(RxApp.MainThreadScheduler);

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

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
        public bool IsPasswordInvalid
        {
            get => _isPasswordInvalid;
            set => this.RaiseAndSetIfChanged(ref _isPasswordInvalid, value);
        }

        private bool _isPasswordInvalid;

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
        public void Dispose()
        {
            _credentialsAuthSessionCts?.Dispose();
            _qrAuthSessionCts?.Dispose();
            _deviceConfirmationCts?.Dispose();
        }

        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) => GetDeviceCodeInteraction.Handle(Unit.Default).ToTask()!;

        /// <inheritdoc />
        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect) => throw new NotImplementedException();

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            if (_qrAuthSessionCts!.IsCancellationRequested)
                DeviceConfirmationInteraction.Handle(_deviceConfirmationCts!.Token).Subscribe();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<AuthPollResult?> AuthenticateAsync()
        {
            _credentialsAuthSessionCts?.Dispose();
            _qrAuthSessionCts?.Dispose();
            _deviceConfirmationCts?.Dispose();
            _credentialsAuthSessionCts = new CancellationTokenSource();
            _qrAuthSessionCts = new CancellationTokenSource();
            _deviceConfirmationCts = new CancellationTokenSource();

            IsPasswordInvalid = false;

            AuthenticateInteraction.Handle(_deviceConfirmationCts.Token).Subscribe();

            AuthSessionDetails details = new() { Authenticator = this };

            try
            {
                QrAuthSession authSession = await _session.SteamClient.Authentication.BeginAuthSessionViaQRAsync(details).ConfigureAwait(false);
                LoginChallenge = authSession.ChallengeURL;
                authSession.ChallengeURLChanged += () => LoginChallenge = authSession.ChallengeURL;
                AuthPollResult result = await authSession.PollingWaitForResultAsync(_qrAuthSessionCts.Token).ConfigureAwait(false);
                await _deviceConfirmationCts.CancelAsync().ConfigureAwait(false);
                return result;
            }
            catch (TaskCanceledException) { }

            if (_credentialsAuthSessionCts.IsCancellationRequested)
                return null;

            details.Username = Username;
            details.Password = Password;
            details.IsPersistentSession = RememberPassword;

            try
            {
                CredentialsAuthSession authSession = await _session.SteamClient.Authentication.BeginAuthSessionViaCredentialsAsync(details).ConfigureAwait(false);
                AuthPollResult result = await authSession.PollingWaitForResultAsync(_credentialsAuthSessionCts.Token).ConfigureAwait(false);
                await _deviceConfirmationCts.CancelAsync().ConfigureAwait(false);
                return result;
            }
            catch (AuthenticationException)
            {
                IsPasswordInvalid = true;
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
    }
}
