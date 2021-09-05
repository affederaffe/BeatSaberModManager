using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlist
{
    public class PlaylistSong
    {
        [JsonPropertyName("key")]
        public string Id { get; init; } = null!;

        [JsonPropertyName("hash")]
        public string Hash { get; init; } = null!;
    }
}