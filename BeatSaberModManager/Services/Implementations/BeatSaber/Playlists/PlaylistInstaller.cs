using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    /// <summary>
    /// Download and install playlists of <see cref="BeatSaverMap"/>s.
    /// </summary>
    public class PlaylistInstaller
    {
        private readonly HttpProgressClient _httpClient;
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistInstaller"/> class.
        /// </summary>
        public PlaylistInstaller(HttpProgressClient httpClient, BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _httpClient = httpClient;
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        /// <summary>
        /// Asynchronously downloads and installs a <see cref="Playlist"/> from an <see cref="Uri"/>.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="uri">The <see cref="Uri"/> to download the playlist from.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public async Task<bool> InstallPlaylistAsync(string installDir, Uri uri, IStatusProgress? progress = null)
        {
            using HttpResponseMessage response = await _httpClient.TryGetAsync(uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return false;
            string playlistsDirPath = Path.Join(installDir, "Playlists");
            string fileName = WebUtility.UrlDecode(uri.Segments.Last());
            string filePath = Path.Join(playlistsDirPath, fileName);
            if (!IOUtils.TryCreateDirectory(playlistsDirPath))
                return false;
            await using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await using Stream writeStream = File.OpenWrite(filePath);
            await contentStream.CopyToAsync(writeStream);
            contentStream.Seek(0, SeekOrigin.Begin);
            Playlist? playlist = await JsonSerializer.DeserializeAsync(contentStream, PlaylistJsonSerializerContext.Default.Playlist);
            return playlist is not null && await InstallPlaylistAsync(installDir, playlist, progress).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously installs a <see cref="Playlist"/> from a file.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="filePath">The path of the <see cref="Playlist"/>'s file.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public async Task<bool> InstallPlaylistAsync(string installDir, string filePath, IStatusProgress? progress = null)
        {
            string fileName = Path.GetFileName(filePath);
            string playlistsDirPath = Path.Join(installDir, "Playlists");
            string destFilePath = Path.Join(playlistsDirPath, fileName);
            if (!IOUtils.TryCreateDirectory(playlistsDirPath))
                return false;
            await using FileStream contentStream = File.OpenRead(filePath);
            await using FileStream writeStream = File.OpenWrite(destFilePath);
            await contentStream.CopyToAsync(writeStream);
            contentStream.Seek(0, SeekOrigin.Begin);
            Playlist? playlist = await JsonSerializer.DeserializeAsync(contentStream, PlaylistJsonSerializerContext.Default.Playlist);
            return playlist is not null && await InstallPlaylistAsync(installDir, playlist, progress).ConfigureAwait(false);
        }

        private async Task<bool> InstallPlaylistAsync(string installDir, Playlist playlist, IStatusProgress? progress = null)
        {
            progress?.Report(0);
            BeatSaverMap[] maps = await GetMapsAsync(playlist).ConfigureAwait(false);
            for (int i = 0; i < maps.Length; i++)
            {
                progress?.Report(new ProgressInfo(StatusType.Installing, maps[i].Name));
                if (maps[i].Versions.Length <= 0) continue;
                HttpResponseMessage response = await _httpClient.TryGetAsync(new Uri(maps[i].Versions.Last().DownloadUrl)).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) continue;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using ZipArchive archive = new(stream);
                bool success = BeatSaverMapInstaller.TryExtractBeatSaverMapToDir(installDir, maps[i], archive);
                if (!success) progress?.Report(new ProgressInfo(StatusType.Failed, maps[i].Name));
                progress?.Report(((double)i + 1) / maps.Length);
            }

            return true;
        }

        private async Task<BeatSaverMap[]> GetMapsAsync(Playlist playlist)
        {
            BeatSaverMap?[] maps = new BeatSaverMap?[playlist.Songs.Length];
            for (int i = 0; i < maps.Length; i++)
            {
                if (!string.IsNullOrEmpty(playlist.Songs[i].Id))
                    maps[i] = await _beatSaverMapInstaller.GetBeatSaverMapByKeyAsync(playlist.Songs[i].Id!).ConfigureAwait(false);
                else if (!string.IsNullOrEmpty(playlist.Songs[i].Hash))
                    maps[i] = await _beatSaverMapInstaller.GetBeatSaverMapByHashAsync(playlist.Songs[i].Hash!).ConfigureAwait(false);
            }

            return maps.Where(static x => x?.Versions.Length > 0).ToArray()!;
        }
    }
}
