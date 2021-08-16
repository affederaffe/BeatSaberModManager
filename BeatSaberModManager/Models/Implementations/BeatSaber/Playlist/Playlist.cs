using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.Playlist
{
    public class Playlist
    {
        [JsonPropertyName("playlistName")]
        public string? PlaylistTitle { get; set; }

        [JsonPropertyName("playlistAuthor")]
        public string? PlaylistAuthor { get; set; }

        [JsonPropertyName("songs")]
        public PlaylistSong[]? Songs { get; set; }
    }
}