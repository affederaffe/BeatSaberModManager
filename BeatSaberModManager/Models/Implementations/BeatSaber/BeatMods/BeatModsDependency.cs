using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsDependency
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("_id")]
        public string Id { get; init; } = null!;

        [JsonIgnore]
        public IMod? DependingMod { get; set; }
    }
}