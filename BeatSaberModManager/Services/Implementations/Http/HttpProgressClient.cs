using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Implementations.Http
{
    public class HttpProgressClient : HttpClient
    {
        public HttpProgressClient(AssemblyName assemblyName)
        {
            DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue(nameof(BeatSaberModManager), assemblyName.Version!.ToString())));
            Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<HttpResponseMessage> GetAsync(string url, IProgress<double>? progress)
        {
            HttpResponseMessage response = await GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            long total = 0;
            long? length = response.Content.Headers.ContentLength;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            MemoryStream ms = length.HasValue ? new MemoryStream((int)length.Value) : new MemoryStream();
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
            response.Content = new StreamContent(ms);
            return response;
        }

        public async IAsyncEnumerable<HttpResponseMessage> GetAsync(IEnumerable<string> urls, IProgress<double>? progress, double length)
        {
            long total = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            foreach (string url in urls)
            {
                HttpResponseMessage response = await GetAsync(url).ConfigureAwait(false);
                MemoryStream ms = new();
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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

        public async IAsyncEnumerable<HttpResponseMessage> GetAsync(IReadOnlyList<string> urls, IProgress<double>? progress)
        {
            progress?.Report(0);
            for (int i = 0; i < urls.Count; i++)
            {
                HttpResponseMessage response = await GetAsync(urls[i]).ConfigureAwait(false);
                progress?.Report(((double)i + 1) / urls.Count);
                yield return response;
            }
        }
    }
}