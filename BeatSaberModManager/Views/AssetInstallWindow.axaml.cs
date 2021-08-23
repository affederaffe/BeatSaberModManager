using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.Views
{
    public partial class AssetInstallWindow : ReactiveWindow<AssetInstallWindowViewModel>
    {
        private readonly string? _uri;

        public AssetInstallWindow()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<AssetInstallWindowViewModel>();
            string installText = (string)this.FindResource("AssetInstallWindow:InstallText")!;
            string log = string.Empty;
            ViewModel.WhenAnyValue(x => x.AssetName)
                .WhereNotNull()
                .Select(x => log = $"{installText} {x}\n{log}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.AssetNameTextBox.Text);
            this.WhenActivated(_ => InstallAsset().ConfigureAwait(false));
        }

        public AssetInstallWindow(string uri) : this()
        {
            _uri = uri;
        }

        private async Task InstallAsset()
        {
            if (_uri is null) return;
            await ViewModel!.InstallAsset(_uri);
            await Task.Delay(1500);
            Close();
        }
    }
}