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
            MemoryStream ms = new();
            await using Stream stream = await header.Content.ReadAsStreamAsync().ConfigureAwait(false);
            while (true)
            {
                int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                if (read <= 0) break;
                await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                total += read;
                if (length.HasValue) progress?.Report(((double)total + 1) / length.Value);
            }

            header.Content = new StreamContent(ms);
            return header;
        }

        public async IAsyncEnumerable<HttpResponseMessage> GetAsync(IEnumerable<string> urls, IProgress<double>? progress)
        {
            progress?.Report(0);
            HttpResponseMessage[] headers = await Task.WhenAll(urls.Select(async x => await GetAsync(x, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false)));
            long? length = headers.Select(x => x.Content.Headers.ContentLength).Sum();
            IAsyncEnumerable<HttpResponseMessage> enumerable = length is > 0 
                ? GetWithLengthAsync(headers, progress, length.Value)
                : GetWithoutLengthAsync(headers, progress);
            await foreach (HttpResponseMessage response in enumerable)
                yield return response;
        }

        private static async IAsyncEnumerable<HttpResponseMessage> GetWithLengthAsync(IEnumerable<HttpResponseMessage> headers, IProgress<double>? progress, double length)
        {
            long total = 0;
            byte[] buffer = new byte[8192];
            foreach (HttpResponseMessage header in headers)
            {
                MemoryStream ms = new();
                await using Stream stream = await header.Content.ReadAsStreamAsync().ConfigureAwait(false);
                while (true)
                {
                    int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    if (read <= 0) break;
                    await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    total += read;
                    progress?.Report(((double)total + 1) / length);
                }

                header.Content = new StreamContent(ms);
                yield return header;
            }
        }

        private static async IAsyncEnumerable<HttpResponseMessage> GetWithoutLengthAsync(IReadOnlyList<HttpResponseMessage> headers, IProgress<double>? progress)
        {
            byte[] buffer = new byte[8192];
            for (int i = 0; i < headers.Count; i++)
            {
                MemoryStream ms = new();
                await using Stream stream = await headers[i].Content.ReadAsStreamAsync().ConfigureAwait(false);
                while (true)
                {
                    int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    if (read <= 0) break;
                    await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    progress?.Report(((double)i + 1) / headers.Count);
                }

                headers[i].Content = new StreamContent(ms);
                yield return headers[i];
            }
        }
    }
}