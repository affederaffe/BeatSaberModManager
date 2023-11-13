using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Serilog;


namespace BeatSaberModManager.Services.Implementations.Http
{
    /// <summary>
    /// A custom <see cref="HttpClient"/> that provides additional overloads that track the progress.
    /// </summary>
    public class HttpProgressClient : HttpClient
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Set the default proxy to an empty one to avoid loading UnityDoorstep's winhttp.
        /// </summary>
        static HttpProgressClient()
        {
            DefaultProxy = new WebProxy();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpProgressClient"/> class with a UserAgent and a default timeout.
        /// </summary>
        public HttpProgressClient(ILogger logger)
        {
            _logger = logger;
            DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue(Program.Product, Program.Version)));
            Timeout = TimeSpan.FromMinutes(3);
        }

        /// <summary>
        /// Try to send a GET request to the specified Uri with an HTTP completion option as an asynchronous operation.
        /// </summary>
        /// <param name="uri">The uri the request is sent to.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> TryGetAsync(Uri uri, IProgress<double>? progress)
        {
            HttpResponseMessage response = await TryGetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return response;
            long total = 0;
            long? length = response.Content.Headers.ContentLength;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            MemoryStream ms = length.HasValue ? new MemoryStream((int)length.Value) : new MemoryStream();
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            while (true)
            {
                int read = await stream.ReadAsync(buffer).ConfigureAwait(false);
                if (read <= 0)
                    break;
                await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                if (!length.HasValue)
                    continue;
                total += read;
                progress?.Report(((double)total + 1) / length.Value);
            }

            ArrayPool<byte>.Shared.Return(buffer);
            ms.Position = 0;
            response.Content = new StreamContent(ms);
            return response;
        }

        /// <summary>
        /// Try to send a GET request to the specified Uri with an HTTP completion option as an asynchronous operation.
        /// </summary>
        /// <param name="uri">The uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> TryGetAsync(Uri uri)
        {
            try
            {
                return await GetAsync(uri).ConfigureAwait(false);
            }
            catch (HttpRequestException httpRequestException)
            {
                _logger.Error(httpRequestException, "Http GET request {Uri} failed", uri);
            }
            catch (TimeoutException timeoutException)
            {
                _logger.Error(timeoutException, "Http GET request {Uri} timed out", uri);
            }

            return new HttpResponseMessage(0);
        }

        /// <summary>
        /// Try to send a GET request to the specified Uri with an HTTP completion option as an asynchronous operation.
        /// </summary>
        /// <param name="uri">The uri the request is sent to.</param>
        /// <param name="completionOption">An HTTP completion option value that indicates when the operation should be considered completed.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> TryGetAsync(Uri uri, HttpCompletionOption completionOption)
        {
            try
            {
                return await GetAsync(uri, completionOption).ConfigureAwait(false);
            }
            catch (HttpRequestException httpRequestException)
            {
                _logger.Error(httpRequestException, "Http GET request {Uri} failed", uri);
            }
            catch (TimeoutException timeoutException)
            {
                _logger.Error(timeoutException, "Http GET request {Uri} timed out", uri);
            }

            return new HttpResponseMessage(0);
        }
    }
}
