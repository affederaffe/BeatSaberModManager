using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Interfaces;


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

        public async Task<bool> InstallPlaylistFromFileAsync(string filePath, IStatusProgress? progress = null)
        {
            string json = await File.ReadAllTextAsync(filePath);
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(json);
            return playlist is not null && await InstallPlaylistAsync(playlist, progress);
        }

        public async Task<bool> InstallPlaylistFromUrlAsync(string url, IStatusProgress? progress = null)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;
            string body = await response.Content.ReadAsStringAsync();
            Playlist? playlist = JsonSerializer.Deserialize<Playlist>(body);
            return playlist is not null && await InstallPlaylistAsync(playlist, progress);
        }

        private async Task<bool> InstallPlaylistAsync(Playlist playlist, IStatusProgress? progress = null)
        {
            for (int i = 0; i < playlist.Songs!.Length; i++)
            {
                progress?.Report((double)i / (playlist.Songs!.Length - 1));
                bool success = await _beatSaverMapInstaller.InstallBeatSaverMapFromKeyAsync(playlist.Songs![i].Id!, progress);
                if (!success) return false;
            }

            return true;
        }
    }
}