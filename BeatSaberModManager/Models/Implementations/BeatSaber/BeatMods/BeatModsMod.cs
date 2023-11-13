using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc cref="BeatSaberModManager.Models.Interfaces.IMod" />
    public class BeatModsMod : IMod
    {
        /// <inheritdoc />
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("version")]
        [JsonConverter(typeof(VersionConverter))]
        public required Version Version { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("description")]
        public required string Description { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("category")]
        public required string Category { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("link")]
        public required Uri MoreInfoLink { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("required")]
        public required bool IsRequired { get; init; }

        /// <summary>
        /// The downloads for the mod.
        /// </summary>
        [JsonPropertyName("downloads")]
        public required IReadOnlyList<BeatModsDownload> Downloads { get; init; }

        /// <summary>
        /// The dependencies of the mod.
        /// </summary>
        [JsonPropertyName("dependencies")]
        public required IReadOnlyList<BeatModsDependency> Dependencies { get; init; }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Name, Version);
    }
}
