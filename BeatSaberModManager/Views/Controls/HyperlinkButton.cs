using System;

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
        public static readonly DirectProperty<HyperlinkButton, Uri?> UriProperty = AvaloniaProperty.RegisterDirect<HyperlinkButton, Uri?>(nameof(Uri), static o => o.Uri, static (o, v) => o.Uri = v);

        /// <summary>
        /// The uri to open.
        /// </summary>
        public Uri? Uri
        {
            get => _uri;
            set => SetAndRaise(UriProperty, ref _uri, value);
        }

        private Uri? _uri;

        /// <summary>
        /// Opens the <see cref="Uri"/> when it's valid and the <see cref="Control"/> is enabled.
        /// </summary>
        protected override void OnClick()
        {
            if (IsEffectivelyEnabled && Uri is not null)
                PlatformUtils.TryOpenUri(Uri);
        }
    }
}
