using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Implementations.JsonSerializerContexts;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    public class PlaylistInstaller
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly HttpProgressClient _httpClient;
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public PlaylistInstaller(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, HttpProgressClient httpClient, BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _httpClient = httpClient;
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public async Task<bool> InstallPlaylistAsync(Uri uri, IStatusProgress? progress = null)
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return false;
            using HttpResponseMessage response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string playlistsDirPath = Path.Combine(_appSettings.Value.InstallDir.Value!, "Playlists");
            string fileName = uri.Segments.Last();
            string filePath = Path.Combine(playlistsDirPath, fileName);
            IOUtils.SafeCreateDirectory(playlistsDirPath);
            await File.WriteAllTextAsync(filePath, body).ConfigureAwait(false);
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(body, PlaylistJsonSerializerContext.Default.Playlist);
            return playlist is not null && await InstallPlaylistAsync(playlist, progress).ConfigureAwait(false);
        }

        public async Task<bool> InstallPlaylistAsync(string filePath, IStatusProgress? progress = null)
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return false;
            string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            string playlistsDirPath = Path.Combine(_appSettings.Value.InstallDir.Value!, "Playlists");
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(playlistsDirPath, fileName);
            IOUtils.SafeCreateDirectory(playlistsDirPath);
            await File.WriteAllTextAsync(destFilePath, json).ConfigureAwait(false);
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(json, PlaylistJsonSerializerContext.Default.Playlist);
            return playlist is not null && await InstallPlaylistAsync(playlist, progress).ConfigureAwait(false);
        }

        private async Task<bool> InstallPlaylistAsync(Playlist playlist, IStatusProgress? progress = null)
        {
            BeatSaverMap?[] maps = await GetMapsAsync(playlist).ConfigureAwait(false);
            if (maps.Any(x => x is null || x.Versions.Length <= 0)) return false;
            string[] urls = maps.Select(x => x!.Versions.Last().DownloadUrl).ToArray();
            int i = 0;
            progress?.Report(maps[i]!.Name);
            progress?.Report(ProgressBarStatusType.Installing);
            await foreach (HttpResponseMessage response in _httpClient.GetAsync(urls, progress).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) return false;
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using ZipArchive archive = new(stream);
                bool success = _beatSaverMapInstaller.ExtractBeatSaverMapToFolder(maps[i]!, archive);
                if (!success) return false;
                if (++i >= maps.Length) return true;
                progress?.Report(maps[i]!.Name);
            }

            return true;
        }

        private async Task<BeatSaverMap?[]> GetMapsAsync(Playlist playlist)
        {
            List<BeatSaverMap?> maps = new(playlist.Songs.Length);
            foreach (PlaylistSong song in playlist.Songs)
            {
                BeatSaverMap? map = await _beatSaverMapInstaller.GetBeatSaverMapAsync(song.Id).ConfigureAwait(false);
                maps.Add(map);
            }

            return maps.ToArray();
        }
    }
}