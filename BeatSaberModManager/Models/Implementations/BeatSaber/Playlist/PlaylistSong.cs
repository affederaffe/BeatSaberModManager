using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.Playlist
{
    public class PlaylistSong
    {
        [JsonPropertyName("key")]
        public string? Id { get; set; }

        [JsonPropertyName("hash")]
        public string? Hash { get; set; }
    }
}