using System.Reactive.Disposables;
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
            this.WhenActivated(disposables =>
            {
                ViewModel.WhenAnyValue(x => x.AssetName)
                    .Select(x => $"{this.FindResource("AssetInstallWindow:InstallText")} {x}")
                    .BindTo(AssetNameTextBox, x => x.Text)
                    .DisposeWith(disposables);
            });
        }

        public AssetInstallWindow(string uri) : this()
        {
            _uri = uri;
        }

        protected override async void OnInitialized()
        {
            if (_uri is null) return;
            await ViewModel!.InstallAsset(_uri);
            await Task.Delay(1500);
            Close();
        }
    }
}