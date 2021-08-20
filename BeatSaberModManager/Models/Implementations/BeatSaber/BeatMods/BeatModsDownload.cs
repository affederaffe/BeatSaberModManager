using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsDownload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        [JsonPropertyName("hashMd5")]
        public BeatModsHash[] Hashes { get; set; } = null!;
    }
}