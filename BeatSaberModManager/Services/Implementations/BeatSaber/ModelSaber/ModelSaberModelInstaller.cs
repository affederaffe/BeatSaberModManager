using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber
{
    /// <summary>
    /// Download and install models from https://modelsaber.com.
    /// </summary>
    public class ModelSaberModelInstaller(HttpProgressClient httpClient)
    {
        /// <summary>
        /// Asynchronously downloads and installs a model from https://modelsaber.com.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="uri">The <see cref="Uri"/> to download the model from.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public async Task<bool> InstallModelAsync(string installDir, Uri uri, IStatusProgress? progress = null)
        {
            ArgumentNullException.ThrowIfNull(uri);
            string? directoryName = GetDirectoryName(uri);
            if (directoryName is null)
                return false;
            string assetDirectoryPath = Path.Join(installDir, directoryName);
            if (!IOUtils.TryCreateDirectory(assetDirectoryPath))
                return false;
            string modelName = WebUtility.UrlDecode(uri.Segments.Last());
            progress?.Report(new ProgressInfo(StatusType.Installing, modelName));
            using HttpResponseMessage response = await httpClient.TryGetAsync(new Uri($"https://modelsaber.com/files/{uri.Host}{uri.LocalPath}"), progress).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return false;
            string assetFilePath = Path.Join(assetDirectoryPath, modelName);
#pragma warning disable CA2007
            await using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await using Stream? writeStream = IOUtils.TryOpenFile(assetFilePath, FileMode.Create, FileAccess.Write);
#pragma warning restore CA2007
            if (writeStream is null)
                return false;
            await contentStream.CopyToAsync(writeStream).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Maps the type of the model to it's respective directory name.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to download the model from.</param>
        /// <returns>The name of the model types directory.</returns>
        private static string? GetDirectoryName(Uri uri) => uri.Host switch
        {
            "avatar" => "CustomAvatars",
            "saber" => "CustomSabers",
            "platform" => "CustomPlatforms",
            "bloq" => "CustomNotes",
            _ => null
        };
    }
}
