using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsDownload
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("hashMd5")]
        public BeatModsHash[]? Hashes { get; set; }
    }
}