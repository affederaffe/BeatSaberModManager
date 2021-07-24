using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMap
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("downloadURL")]
        public string? DownloadUrl { get; set; }

        [JsonPropertyName("coverURL")]
        public string? CoverUrl { get; set; }

        [JsonPropertyName("metadata")]
        public BeatSaverMapMetaData? MetaData { get; set; }
    }
}