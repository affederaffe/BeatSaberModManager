using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlists
{
    public class PlaylistSong
    {
        [JsonPropertyName("key")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = null!;
    }
}