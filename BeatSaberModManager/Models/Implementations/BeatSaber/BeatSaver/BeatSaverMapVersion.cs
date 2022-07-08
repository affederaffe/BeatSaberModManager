using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    /// <summary>
    /// A version of a <see cref="BeatSaverMap"/>.
    /// </summary>
    public class BeatSaverMapVersion
    {
        /// <summary>
        /// The url to download the map from.
        /// </summary>
        [JsonPropertyName("downloadURL")]
        public string DownloadUrl { get; set; } = null!;
    }
}
