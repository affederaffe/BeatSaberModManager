using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.BeatMods
{
    public class BeatModsAuthor
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("userName")]
        public string? Username { get; set; }
    }
}