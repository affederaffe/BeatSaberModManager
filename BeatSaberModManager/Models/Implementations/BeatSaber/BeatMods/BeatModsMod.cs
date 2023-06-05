using System;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc cref="BeatSaberModManager.Models.Interfaces.IMod" />
    public class BeatModsMod : IMod, IEquatable<BeatModsMod>
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
        public required string MoreInfoLink { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("required")]
        public required bool IsRequired { get; init; }

        /// <summary>
        /// The downloads for the mod.
        /// </summary>
        [JsonPropertyName("downloads")]
        public required BeatModsDownload[] Downloads { get; init; }

        /// <summary>
        /// The dependencies of the mod.
        /// </summary>
        [JsonPropertyName("dependencies")]
        public required BeatModsDependency[] Dependencies { get; init; }

        /// <inheritdoc />
        public bool Equals(BeatModsMod? other) => other is not null && (ReferenceEquals(this, other) || (Name == other.Name && Version.Equals(other.Version)));

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is not null && (ReferenceEquals(this, obj) || (obj is BeatModsMod beatModsMod && Equals(beatModsMod)));

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Name, Version);
    }
}
