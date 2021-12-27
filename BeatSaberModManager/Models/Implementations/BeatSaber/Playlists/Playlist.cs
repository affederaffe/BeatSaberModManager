using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlists
{
    public class Playlist
    {
        [JsonPropertyName("playlistName")]
        public string PlaylistTitle { get; set; } = null!;

        [JsonPropertyName("playlistAuthor")]
        public string PlaylistAuthor { get; set; } = null!;

        [JsonPropertyName("songs")]
        public PlaylistSong[] Songs { get; set; } = null!;
    }
}