using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMap
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("metadata")]
        public BeatSaverMapMetaData? MetaData { get; set; }

        [JsonPropertyName("versions")]
        public BeatSaverMapVersion[]? Versions { get; set; }
    }
}