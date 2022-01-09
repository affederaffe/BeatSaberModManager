using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.GitHub
{
    public class Asset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; } = null!;
    }
}