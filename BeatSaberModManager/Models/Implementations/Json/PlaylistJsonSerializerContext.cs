using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.Playlists;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <inheritdoc />
    [JsonSerializable(typeof(Playlist))]
    internal partial class PlaylistJsonSerializerContext : JsonSerializerContext { }
}