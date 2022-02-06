using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlists
{
    /// <summary>
    /// A song of a <see cref="Playlist"/>
    /// </summary>
    public class PlaylistSong
    {
        /// <summary>
        /// The map's unique identifier on https://beatsaver.com
        /// </summary>
        [JsonPropertyName("key")]
        public string Id { get; set; } = null!;
    }
}