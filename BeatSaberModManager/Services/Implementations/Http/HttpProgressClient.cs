using System;
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
            byte[] buffer = new byte[8192];
            await using MemoryStream ms = new();
            await using Stream stream = await header.Content.ReadAsStreamAsync().ConfigureAwait(false);
            while (true)
            {
                int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                if (read <= 0) break;
                await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                total += read;
                if (length.HasValue) progress?.Report(((double)total + 1) / length.Value);
            }

            header.Content = new ReadOnlyMemoryContent(ms.ToArray());
            return header;
        }

        public async IAsyncEnumerable<HttpResponseMessage> GetAsync(IEnumerable<string> urls, IProgress<double>? progress)
        {
            HttpResponseMessage[] headers = await Task.WhenAll(urls.Select(async x => await GetAsync(x, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false)));
            long total = 0;
            long? length = headers.Select(x => x.Content.Headers.ContentLength).Sum();
            byte[] buffer = new byte[8192];
            await using MemoryStream ms = new();
            foreach (HttpResponseMessage header in headers)
            {
                ms.SetLength(0);
                await using Stream stream = await header.Content.ReadAsStreamAsync().ConfigureAwait(false);
                while (true)
                {
                    int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    if (read <= 0) break;
                    await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    total += read;
                    if (length.HasValue) progress?.Report(((double)total + 1) / length.Value);
                }

                header.Content = new ReadOnlyMemoryContent(ms.ToArray());
                yield return header;
            }
        }
    }
}