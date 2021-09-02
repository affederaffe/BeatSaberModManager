using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;

using Microsoft.Extensions.Options;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlist
{
    public class PlaylistInstaller
    {
        private readonly SettingsStore _settingsStore;
        private readonly HttpClient _httpClient;
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public PlaylistInstaller(IOptions<SettingsStore> settingsStore, HttpClient httpClient, BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _settingsStore = settingsStore.Value;
            _httpClient = httpClient;
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public async Task<bool> InstallPlaylistAsync(string[] filePaths, IStatusProgress? progress = null)
        {
            if (filePaths.Length is 1) return await InstallPlaylistAsync(filePaths[0], progress);
            IEnumerable<Task<bool>> tasks = filePaths.Select(async x => await InstallPlaylistAsync(x, progress).ConfigureAwait(false));
            bool[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.All(x => x);
        }

        public async Task<bool> InstallPlaylistAsync(Uri uri, IStatusProgress? progress = null)
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            using HttpResponseMessage response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string playlistsDirPath = Path.Combine(_settingsStore.InstallDir, "Playlists");
            string fileName = uri.Segments.Last();
            string filePath = Path.Combine(playlistsDirPath, fileName);
            if (!Directory.Exists(playlistsDirPath)) Directory.CreateDirectory(playlistsDirPath);
            await File.WriteAllTextAsync(filePath, body).ConfigureAwait(false);
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(body);
            return playlist is not null && await InstallPlaylistAsync(playlist, progress).ConfigureAwait(false);
        }

        private async Task<bool> InstallPlaylistAsync(string filePath, IStatusProgress? progress = null)
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            string playlistsDirPath = Path.Combine(_settingsStore.InstallDir, "Playlists");
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(playlistsDirPath, fileName);
            if (!Directory.Exists(playlistsDirPath)) Directory.CreateDirectory(playlistsDirPath);
            await File.WriteAllTextAsync(destFilePath, json).ConfigureAwait(false);
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(json);
            return playlist is not null && await InstallPlaylistAsync(playlist, progress).ConfigureAwait(false);
        }

        private async Task<bool> InstallPlaylistAsync(Playlist playlist, IStatusProgress? progress = null)
        {
            for (int i = 0; i < playlist.Songs.Length; i++)
            {
                bool success = await _beatSaverMapInstaller.InstallBeatSaverMapAsync(playlist.Songs[i].Id, progress).ConfigureAwait(false);
                if (!success) return false;
                progress?.Report(((double)i + 1) / playlist.Songs.Length);
            }

            return true;
        }
    }
}