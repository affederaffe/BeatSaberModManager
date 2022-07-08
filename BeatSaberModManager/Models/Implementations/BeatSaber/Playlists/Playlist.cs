using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlists
{
    /// <summary>
    /// A playlist of multiple songs.
    /// </summary>
    public class Playlist
    {
        /// <summary>
        /// The title of the playlist.
        /// </summary>
        [JsonPropertyName("playlistName")]
        public string PlaylistTitle { get; set; } = null!;

        /// <summary>
        /// The author of the playlist.
        /// </summary>
        [JsonPropertyName("playlistAuthor")]
        public string PlaylistAuthor { get; set; } = null!;

        /// <summary>
        /// The songs included in the playlist.
        /// </summary>
        [JsonPropertyName("songs")]
        public PlaylistSong[] Songs { get; set; } = null!;
    }
}
