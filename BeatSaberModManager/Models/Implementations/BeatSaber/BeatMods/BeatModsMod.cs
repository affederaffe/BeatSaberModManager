using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsMod : IMod
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("version")]
        public string Version { get; init; } = null!;

        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; init; } = null!;

        [JsonPropertyName("_id")]
        public string Id { get; init; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; init; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; init; } = null!;

        [JsonPropertyName("category")]
        public string Category { get; init; } = null!;

        [JsonPropertyName("link")]
        public string MoreInfoLink { get; init; } = null!;

        [JsonPropertyName("author")]
        public BeatModsAuthor Author { get; init; } = null!;

        [JsonPropertyName("downloads")]
        public BeatModsDownload[] Downloads { get; init; } = null!;

        [JsonPropertyName("dependencies")]
        public BeatModsDependency[] Dependencies { get; init; } = null!;
    }
}