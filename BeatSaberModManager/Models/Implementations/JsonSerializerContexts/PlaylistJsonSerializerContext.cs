using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.BeatSaber.Playlists;


namespace BeatSaberModManager.Models.Implementations.JsonSerializerContexts
{
    [JsonSerializable(typeof(Playlist))]
    public partial class PlaylistJsonSerializerContext : JsonSerializerContext { }
}