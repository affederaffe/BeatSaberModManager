using System;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsMod : IMod
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; } = null!;

        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = null!;

        [JsonPropertyName("_id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        [JsonPropertyName("link")]
        public string MoreInfoLink { get; set; } = null!;

        [JsonPropertyName("author")]
        public BeatModsAuthor Author { get; set; } = null!;

        [JsonPropertyName("downloads")]
        public BeatModsDownload[] Downloads { get; set; } = null!;

        [JsonPropertyName("dependencies")]
        public BeatModsDependency[] Dependencies { get; set; } = null!;
    }
}