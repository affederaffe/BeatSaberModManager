using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;


namespace BeatSaberModManager.Views.Helpers
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class AsyncImageCache
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly Dictionary<Uri, Task<Bitmap>> _loadingQueue = [];
        private static readonly Dictionary<Uri, Bitmap> _cache = [];
        private static readonly PixelSize _imageSize = new(200, 200);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Bitmap> LoadImageAsync(Uri uri, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(uri);
            if (_cache.TryGetValue(uri, out Bitmap? bitmap))
                return bitmap;
            if (_loadingQueue.TryGetValue(uri, out Task<Bitmap>? task))
                return await task.ConfigureAwait(true);
            task = Task.Run(() => LoadNonCachedImageAsync(uri, cancellationToken), cancellationToken);
            _loadingQueue.Add(uri, task);
            bitmap = await task.ConfigureAwait(true);
            _loadingQueue.Remove(uri);
            _cache.Add(uri, bitmap);
            return bitmap;
        }

        private static async Task<Bitmap> LoadNonCachedImageAsync(Uri uri, CancellationToken cancellationToken)
        {
            Stream stream = await _httpClient.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
            using MemoryStream memoryStream = new();
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;
            return await Dispatcher.UIThread.InvokeAsync(() =>
            {
                using Bitmap largeBitmap = new(memoryStream);
                return largeBitmap.CreateScaledBitmap(_imageSize);
            });
        }
    }
}
