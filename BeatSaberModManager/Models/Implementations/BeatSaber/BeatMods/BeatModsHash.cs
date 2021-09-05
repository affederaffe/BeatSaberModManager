using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsHash
    {
        [JsonPropertyName("hash")]
        public string Hash { get; init; } = null!;

        [JsonPropertyName("file")]
        public string File { get; init; } = null!;
    }
}