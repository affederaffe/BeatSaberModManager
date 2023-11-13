using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    /// <summary>
    /// A Beatmap from https://beatsaver.com.
    /// </summary>
    public class BeatSaverMap
    {
        /// <summary>
        /// The map's unique identifier on https://beatsaver.com.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        /// <summary>
        /// The name of the map.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        [JsonPropertyName("metadata")]
        public required BeatSaverMapMetaData MetaData { get; init; }

        /// <summary>
        /// All versions of the map.<br/>
        /// The last entry corresponds to the latest version.
        /// </summary>
        [JsonPropertyName("versions")]
        public required IReadOnlyList<BeatSaverMapVersion> Versions { get; init; }
    }
}
