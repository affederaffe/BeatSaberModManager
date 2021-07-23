using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsMod : IMod
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("gameVersion")]
        public string? GameVersion { get; set; }

        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("link")]
        public string? MoreInfoLink { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("author")]
        public IAuthor? Author { get; set; }

        [JsonPropertyName("downloads")]
        public IDownload[]? Downloads { get; set; }

        [JsonPropertyName("dependencies")]
        public IDependency[]? Dependencies { get; set; }
    }
}