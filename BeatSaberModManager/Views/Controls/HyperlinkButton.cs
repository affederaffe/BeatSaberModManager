using Avalonia;
using Avalonia.Controls;

using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Views.Controls
{
    public class HyperlinkButton : Button
    {
        public static readonly DirectProperty<HyperlinkButton, string?> UriProperty = AvaloniaProperty.RegisterDirect<HyperlinkButton, string?>(nameof(Uri), static o => o.Uri, static (o, v) => o.Uri = v);

        private string? _uri;
        public string? Uri
        {
            get => _uri;
            set => SetAndRaise(UriProperty, ref _uri, value);
        }

        protected override void OnClick()
        {
            if (!IsEffectivelyEnabled || Uri is null) return;
            PlatformUtils.OpenUri(Uri);
        }
    }
}