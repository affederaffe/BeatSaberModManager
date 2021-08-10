using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.Views
{
    public class AssetInstallWindow : ReactiveWindow<AssetInstallWindowViewModel>
    {
        private readonly string? _uri;

        public AssetInstallWindow()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<AssetInstallWindowViewModel>();
            TextBox assetNameTextBox = this.FindControl<TextBox>("AssetNameTextBox");
            ViewModel.WhenAnyValue(x => x.AssetName)
                     .Select(x => $"{this.FindResource("AssetInstallWindow:InstallText")} {x}")
                     .BindTo(assetNameTextBox, x => x.Text);
            this.WhenActivated(_ => InstallAssetAndClose().ConfigureAwait(false));
        }

        public AssetInstallWindow(string uri) : this()
        {
            _uri = uri;
        }

        private async Task InstallAssetAndClose()
        {
            if (_uri is null) return;
            await ViewModel!.InstallAsset(_uri);
            await Task.Delay(1500);
            Close();
        }
    }
}