using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlist
{
    public class PlaylistInstaller
    {
        private readonly HttpClient _httpClient;
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public PlaylistInstaller(HttpClient httpClient, BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _httpClient = httpClient;
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public async Task<bool> InstallPlaylistsAsync(IEnumerable<string> filePaths)
        {
            foreach (string filePath in filePaths.Where(File.Exists))
            {
                string json = await File.ReadAllTextAsync(filePath);
                Playlist? playlist = JsonSerializer.Deserialize<Playlist>(json);
                if (playlist is null) continue;
                bool success = await InstallPlaylistAsync(playlist);
                if (!success) return false;
            }

            return true;
        }

        public async Task<bool> InstallPlaylistAsync(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync();
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(body);
            return playlist is not null && await InstallPlaylistAsync(playlist);
        }

        private async Task<bool> InstallPlaylistAsync(Playlist playlist)
        {
            foreach (PlaylistSong song in playlist.Songs!)
            {
                bool success = await _beatSaverMapInstaller.InstallBeatSaverMapFromKeyAsync(song.Id!);
                if (!success) return false;
            }

            return true;
        }
    }
}