using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlist
{
    public class Playlist
    {
        [JsonPropertyName("playlistName")]
        public string PlaylistTitle { get; init; } = null!;

        [JsonPropertyName("playlistAuthor")]
        public string PlaylistAuthor { get; init; } = null!;

        [JsonPropertyName("songs")]
        public PlaylistSong[] Songs { get; init; } = null!;
    }
}