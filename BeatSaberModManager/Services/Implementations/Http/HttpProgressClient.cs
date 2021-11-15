using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Implementations.Http
{
    public class HttpProgressClient : HttpClient
    {
        public async Task<HttpResponseMessage> GetAsync(string url, IProgress<double>? progress)
        {
            HttpResponseMessage header = await base.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            long total = 0;
            long? length = header.Content.Headers.ContentLength;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            MemoryStream ms = new();
            await using Stream stream = await header.Content.ReadAsStreamAsync().ConfigureAwait(false);
            while (true)
            {
                int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                if (read <= 0) break;
                await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                total += read;
                if (!length.HasValue) continue;
                progress?.Report(((double)total + 1) / length.Value);
            }

            ArrayPool<byte>.Shared.Return(buffer);
            header.Content = new StreamContent(ms);
            return header;
        }

        public async IAsyncEnumerable<HttpResponseMessage> GetAsync(IEnumerable<string> urls, IProgress<double>? progress)
        {
            progress?.Report(0);
            Uri[] uris = urls.Select(x => new Uri(x)).ToArray();
            long? length = await GetLengthAsync(uris);
            IAsyncEnumerable<HttpResponseMessage> e = length > 0 ? GetWithLengthAsync(uris, progress, length.Value) : GetWithoutLengthAsync(uris, progress);
            await foreach (HttpResponseMessage response in e.ConfigureAwait(false))
                yield return response;
        }

        private async Task<long?> GetLengthAsync(IEnumerable<Uri> uris)
        {
            long? result = 0;
            foreach (Uri uri in uris)
            {
                using HttpResponseMessage header = await GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                result += header.Content.Headers.ContentLength;
            }

            return result;
        }

        private async IAsyncEnumerable<HttpResponseMessage> GetWithLengthAsync(IEnumerable<Uri> uris, IProgress<double>? progress, double length)
        {
            long total = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            foreach (Uri uri in uris)
            {
                HttpResponseMessage response = await GetAsync(uri);
                MemoryStream ms = new();
                await using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                while (true)
                {
                    int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    if (read <= 0) break;
                    await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    total += read;
                    progress?.Report(((double)total + 1) / length);
                }

                response.Content = new StreamContent(ms);
                yield return response;
            }

            ArrayPool<byte>.Shared.Return(buffer);
        }

        private async IAsyncEnumerable<HttpResponseMessage> GetWithoutLengthAsync(IReadOnlyList<Uri> uris, IProgress<double>? progress)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            for (int i = 0; i < uris.Count; i++)
            {
                HttpResponseMessage response = await GetAsync(uris[i]);
                MemoryStream ms = new();
                await using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                while (true)
                {
                    int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    if (read <= 0) break;
                    await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    progress?.Report(((double)i + 1) / uris.Count);
                }

                response.Content = new StreamContent(ms);
                yield return response;
            }

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}