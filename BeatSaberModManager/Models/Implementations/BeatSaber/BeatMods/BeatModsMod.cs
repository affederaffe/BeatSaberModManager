using System;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc />
    public class BeatModsMod : IMod
    {
        /// <inheritdoc />
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        /// <inheritdoc />
        [JsonPropertyName("link")]
        public string MoreInfoLink { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("downloads")]
        public BeatModsDownload[] Downloads { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dependencies")]
        public BeatModsDependency[] Dependencies { get; set; } = null!;
    }
}