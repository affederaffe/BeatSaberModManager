using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Implementations.Http
{
    /// <summary>
    /// A custom <see cref="HttpClient"/> that provides additional overloads that track the progress.
    /// </summary>
    public class HttpProgressClient : HttpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpProgressClient"/> class with a UserAgent and 30s timeout.
        /// </summary>
        public HttpProgressClient()
        {
            DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue(ThisAssembly.Info.Product, ThisAssembly.Info.Version)));
            Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Send a GET request to the specified Uri with an HTTP completion option as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url the request is sent to.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Send multiple GET requests to the specified Uris with an HTTP completion option as an asynchronous operation
        /// </summary>
        /// <param name="urls">The urls the requests are sent to.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <param name="length">The sum of all the content's length</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Send multiple GET requests to the specified Uris with an HTTP completion option as an asynchronous operation
        /// </summary>
        /// <param name="urls">The urls the requests are sent to.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
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