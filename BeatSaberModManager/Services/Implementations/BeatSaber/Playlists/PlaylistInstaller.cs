using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlists;
using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    public class PlaylistInstaller
    {
        private readonly HttpProgressClient _httpClient;
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public PlaylistInstaller(HttpProgressClient httpClient, BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _httpClient = httpClient;
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public async Task<bool> InstallPlaylistAsync(string installDir, Uri uri, IStatusProgress? progress = null)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string playlistsDirPath = Path.Combine(installDir, "Playlists");
            string fileName = uri.Segments.Last();
            string filePath = Path.Combine(playlistsDirPath, fileName);
            IOUtils.TryCreateDirectory(playlistsDirPath);
            await File.WriteAllTextAsync(filePath, body).ConfigureAwait(false);
            Playlist? playlist = JsonSerializer.Deserialize(body, PlaylistJsonSerializerContext.Default.Playlist);
            return playlist is not null && await InstallPlaylistAsync(installDir, playlist, progress).ConfigureAwait(false);
        }

        public async Task<bool> InstallPlaylistAsync(string installDir, string filePath, IStatusProgress? progress = null)
        {
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            string playlistsDirPath = Path.Combine(installDir, "Playlists");
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(playlistsDirPath, fileName);
            IOUtils.TryCreateDirectory(playlistsDirPath);
            await File.WriteAllTextAsync(destFilePath, json).ConfigureAwait(false);
            Playlist? playlist = JsonSerializer.Deserialize(json, PlaylistJsonSerializerContext.Default.Playlist);
            return playlist is not null && await InstallPlaylistAsync(installDir, playlist, progress).ConfigureAwait(false);
        }

        private async Task<bool> InstallPlaylistAsync(string installDir, Playlist playlist, IStatusProgress? progress = null)
        {
            BeatSaverMap[] maps = await GetMapsAsync(playlist).ConfigureAwait(false);
            string[] urls = maps.Select(static x => x.Versions.Last().DownloadUrl).ToArray();
            int i = 0;
            progress?.Report(new ProgressInfo(StatusType.Installing, maps[i].Name));
            await foreach (HttpResponseMessage response in _httpClient.GetAsync(urls, progress).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) return false;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using ZipArchive archive = new(stream);
                bool success = BeatSaverMapInstaller.ExtractBeatSaverMapToDir(installDir, maps[i], archive);
                if (!success) progress?.Report(new ProgressInfo(StatusType.Failed, maps[i].Name));
                if (++i >= maps.Length) return true;
                progress?.Report(new ProgressInfo(StatusType.Installing, maps[i].Name));
            }

            return true;
        }

        private async Task<BeatSaverMap[]> GetMapsAsync(Playlist playlist)
        {
            BeatSaverMap?[] maps = new BeatSaverMap?[playlist.Songs.Length];
            for (int i = 0; i < maps.Length; i++)
                maps[i] = await _beatSaverMapInstaller.GetBeatSaverMapAsync(playlist.Songs[i].Id).ConfigureAwait(false);
            return maps.Where(static x => x?.Versions.Length is > 0).ToArray()!;
        }
    }
}