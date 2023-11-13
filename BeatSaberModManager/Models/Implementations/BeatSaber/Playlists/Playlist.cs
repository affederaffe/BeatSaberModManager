using System.Collections.Generic;
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
        [JsonPropertyName("playlistTitle")]
        public required string PlaylistTitle { get; init; }

        /// <summary>
        /// The author of the playlist.
        /// </summary>
        [JsonPropertyName("playlistAuthor")]
        public required string PlaylistAuthor { get; init; }

        /// <summary>
        /// The songs included in the playlist.
        /// </summary>
        [JsonPropertyName("songs")]
        public required IReadOnlyList<PlaylistSong> Songs { get; init; }
    }
}
