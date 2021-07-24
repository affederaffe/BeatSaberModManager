using System.Reactive.Linq;

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
        public AssetInstallWindow()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<AssetInstallWindowViewModel>();
            TextBox assetNameTextBox = this.FindControl<TextBox>("AssetNameTextBox");
            ViewModel.WhenAnyValue(x => x.AssetName)
                     .Select(x => $"{this.FindResource("AssetInstallWindow:InstallText")} {x}")
                     .BindTo(assetNameTextBox, x => x.Text);
            this.WhenActivated(async _ =>
            {
                if (Uri is null) return;
                await ViewModel.InstallAsset(Uri);
            });
        }

        public string? Uri { private get; init; }
    }
}