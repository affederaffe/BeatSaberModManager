using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverMapVersion
    {
        [JsonPropertyName("downloadURL")]
        public string? DownloadUrl { get; set; }
    }
}