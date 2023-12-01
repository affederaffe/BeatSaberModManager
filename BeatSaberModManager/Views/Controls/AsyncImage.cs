using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using BeatSaberModManager.Views.Helpers;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// An image control that asynchronously retrieves an image using a <see cref="Uri"/>.
    /// </summary>
    [TemplatePart("PART_Image", typeof(Image))]
    [TemplatePart("PART_PlaceholderImage", typeof(Image))]
    public class AsyncImage : TemplatedControl, IDisposable
    {
        /// <summary>
        /// Defines the <see cref="PlaceholderSource"/> property.
        /// </summary>
        public static readonly StyledProperty<IImage?> PlaceholderSourceProperty = AvaloniaProperty.Register<AsyncImage, IImage?>(nameof(PlaceholderSource));

        /// <summary>
        /// Defines the <see cref="Source"/> property.
        /// </summary>
        public static readonly StyledProperty<Uri?> SourceProperty = AvaloniaProperty.Register<AsyncImage, Uri?>(nameof(Source));

        /// <summary>
        /// Defines the <see cref="Stretch"/> property.
        /// </summary>
        public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<AsyncImage, Stretch>(nameof(Stretch), Stretch.Uniform);

        /// <summary>
        /// Defines the <see cref="PlaceholderStretch"/> property.
        /// </summary>
        public static readonly StyledProperty<Stretch> PlaceholderStretchProperty = AvaloniaProperty.Register<AsyncImage, Stretch>(nameof(PlaceholderStretch), Stretch.Uniform);

        /// <summary>
        /// Defines the <see cref="ImageTransition"/> property.
        /// </summary>
        public static readonly StyledProperty<IPageTransition?> ImageTransitionProperty = AvaloniaProperty.Register<AsyncImage, IPageTransition?>(nameof(ImageTransition), new CrossFade(TimeSpan.FromSeconds(0.25)));

        /// <summary>
        /// Defines the <see cref="State"/> property.
        /// </summary>
        public static readonly DirectProperty<AsyncImage, AsyncImageState> StateProperty = AvaloniaProperty.RegisterDirect<AsyncImage, AsyncImageState>(nameof(State), static o => o.State, static (o, v) => o.State = v);

        private bool _isInitialized;
        private AsyncImageState _state;
        private Uri? _currentSource;
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Gets or sets the placeholder image.
        /// </summary>
        public IImage? PlaceholderSource
        {
            get => GetValue(PlaceholderSourceProperty);
            set => SetValue(PlaceholderSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the uri pointing to the image resource
        /// </summary>
        public Uri? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Gets or sets a value controlling how the image will be stretched.
        /// </summary>
        public Stretch Stretch
        {
            get => GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        /// <summary>
        /// Gets or sets a value controlling how the placeholder will be stretched.
        /// </summary>
        public Stretch PlaceholderStretch
        {
            get => GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        /// <summary>
        /// Gets or sets the transition to run when the image is loaded.
        /// </summary>
        public IPageTransition? ImageTransition
        {
            get => GetValue(ImageTransitionProperty);
            set => SetValue(ImageTransitionProperty, value);
        }

        /// <summary>
        /// Gets the current loading state of the image.
        /// </summary>
        public AsyncImageState State
        {
            get => _state;
            private set => SetAndRaise(StateProperty, ref _state, value);
        }

        /// <summary>
        /// TODO
        /// </summary>
        protected Image? ImagePart { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        protected Image? PlaceholderPart { get; private set; }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            base.OnApplyTemplate(e);

            ImagePart = e.NameScope.Get<Image>("PART_Image");
            PlaceholderPart = e.NameScope.Get<Image>("PART_PlaceholderImage");

            _isInitialized = true;

            if (Source is not null && Source != _currentSource)
                SetSource(Source);
        }

        private async void SetSource(Uri? source)
        {
            if (!_isInitialized)
                return;

            if (source is null)
            {
                _currentSource = null;
                State = AsyncImageState.Unloaded;
                AttachSource(null);
                return;
            }

            if (!source.IsAbsoluteUri)
                throw new UriFormatException($"Relative paths aren't supported. Uri:{source}");

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            State = AsyncImageState.Loading;
            _currentSource = source;

            switch (source.Scheme)
            {
                case "http" or "https":
                    try
                    {
#pragma warning disable CA2000
                        Bitmap bitmap = await AsyncImageCache.LoadImageAsync(source, _cancellationTokenSource.Token).ConfigureAwait(true);
#pragma warning restore CA2000
                        AttachSource(bitmap);
                        State = AsyncImageState.Loaded;
                    }
                    catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
                    {
                        _currentSource = null;
                        State = AsyncImageState.Failed;
                        AttachSource(null);
                    }

                    break;
                case "avares":
                    try
                    {
#pragma warning disable CA2007
                        await using Stream stream = AssetLoader.Open(source);
#pragma warning restore CA2007
#pragma warning disable CA2000
                        AttachSource(new Bitmap(stream));
#pragma warning restore CA2000
                        State = AsyncImageState.Loaded;
                    }
                    catch (Exception e) when (e is TaskCanceledException or FileNotFoundException)
                    {
                        _currentSource = null;
                        State = AsyncImageState.Failed;
                        AttachSource(null);
                    }

                    break;
                case "file":
                    try
                    {
#pragma warning disable CA2000
                        AttachSource(new Bitmap(source.LocalPath));
#pragma warning restore CA2000
                        State = AsyncImageState.Loaded;
                    }
                    catch (Exception e) when (e is TaskCanceledException or ArgumentException)
                    {
                        _currentSource = null;
                        State = AsyncImageState.Failed;
                        AttachSource(null);
                    }

                    break;
                default:
                    throw new UriFormatException($"Uri has unsupported scheme. Uri:{source}");
            }
        }

        private void AttachSource(IImage? image)
        {
            if (ImagePart is not null)
            {
                if (ImagePart.Source is IDisposable disposable)
                    disposable.Dispose();
                ImagePart.Source = image;
            }

            if (image is null)
                ImageTransition?.Start(ImagePart, PlaceholderPart, true, _cancellationTokenSource!.Token);
            else if (image.Size != default)
                ImageTransition?.Start(PlaceholderPart, ImagePart, true, _cancellationTokenSource!.Token);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            _cancellationTokenSource?.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public enum AsyncImageState
        {
            /// <summary>
            /// TODO
            /// </summary>
            Unloaded,

            /// <summary>
            /// 
            /// </summary>
            Loading,

            /// <summary>
            /// 
            /// </summary>
            Loaded,

            /// <summary>
            /// 
            /// </summary>
            Failed
        }
    }
}
