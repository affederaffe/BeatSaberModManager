using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsHash : IHash
    {
        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("file")]
        public string? File { get; set; }
    }
}