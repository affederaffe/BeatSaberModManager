using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsAuthor
    {
        [JsonPropertyName("_id")]
        public string Id { get; init; } = null!;

        [JsonPropertyName("userName")]
        public string Username { get; init; } = null!;
    }
}