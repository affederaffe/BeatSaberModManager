using System;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.GitHub
{
    /// <summary>
    /// An asset of a <see cref="Release"/>.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// The name of the asset.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        /// <summary>
        /// The url to download the asset from.
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        public required Uri DownloadUrl { get; init; }
    }
}
