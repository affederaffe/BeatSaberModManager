using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.GitHub
{
    /// <summary>
    /// A release on https://github.com.
    /// </summary>
    public class Release
    {
        /// <summary>
        /// The tag of the <see cref="Release"/>.
        /// </summary>
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = null!;

        /// <summary>
        /// All assets of the <see cref="Release"/>.
        /// </summary>
        [JsonPropertyName("assets")]
        public Asset[] Assets { get; set; } = null!;
    }
}