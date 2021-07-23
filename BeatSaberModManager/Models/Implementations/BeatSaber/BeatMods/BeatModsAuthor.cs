using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsAuthor : IAuthor
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("userName")]
        public string? Username { get; set; }
    }
}