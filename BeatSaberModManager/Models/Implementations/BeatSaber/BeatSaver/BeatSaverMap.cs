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
        public string Id { get; set; } = null!;

        /// <summary>
        /// The name of the map.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Additional metadata.
        /// </summary>
        [JsonPropertyName("metadata")]
        public BeatSaverMapMetaData MetaData { get; set; } = null!;

        /// <summary>
        /// All versions of the map.<br/>
        /// The last entry corresponds to the latest version.
        /// </summary>
        [JsonPropertyName("versions")]
        public BeatSaverMapVersion[] Versions { get; set; } = null!;
    }
}