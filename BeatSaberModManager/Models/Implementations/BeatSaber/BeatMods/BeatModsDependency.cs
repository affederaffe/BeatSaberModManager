using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsDependency
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("_id")]
        public string Id { get; set; } = null!;

        [JsonIgnore]
        public IMod? DependingMod { get; set; }
    }
}