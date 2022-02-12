using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.Playlists;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(Playlist))]
    internal partial class PlaylistJsonSerializerContext : JsonSerializerContext { }
}