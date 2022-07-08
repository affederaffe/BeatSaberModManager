using Avalonia;
using Avalonia.Controls;

using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// A button that opens the specified <see cref="Uri"/> when clicked.
    /// </summary>
    public class HyperlinkButton : Button
    {
        /// <summary>
        /// Defines the <see cref="Uri"/> property.
        /// </summary>
        public static readonly DirectProperty<HyperlinkButton, string?> UriProperty = AvaloniaProperty.RegisterDirect<HyperlinkButton, string?>(nameof(Uri), static o => o.Uri, static (o, v) => o.Uri = v);

        /// <summary>
        /// The uri to open.
        /// </summary>
        public string? Uri
        {
            get => _uri;
            set => SetAndRaise(UriProperty, ref _uri, value);
        }

        private string? _uri;

        /// <summary>
        /// Opens the <see cref="Uri"/> when it's valid and the <see cref="Control"/> is enabled.
        /// </summary>
        protected override void OnClick()
        {
            if (!IsEffectivelyEnabled || Uri is null) return;
            PlatformUtils.TryOpenUri(Uri);
        }
    }
}
