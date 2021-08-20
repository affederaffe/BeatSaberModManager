using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsHash
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = null!;

        [JsonPropertyName("file")]
        public string File { get; set; } = null!;
    }
}