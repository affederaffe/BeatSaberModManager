using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsDownload
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("hashMd5")]
        public BeatModsHash[] Hashes { get; init; } = null!;
    }
}