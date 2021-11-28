using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMap
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("metadata")]
        public BeatSaverMapMetaData MetaData { get; set; } = null!;

        [JsonPropertyName("versions")]
        public BeatSaverMapVersion[] Versions { get; set; } = null!;
    }
}