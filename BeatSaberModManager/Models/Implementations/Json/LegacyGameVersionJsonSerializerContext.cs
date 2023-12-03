using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Versions;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(SteamGameVersion[]))]
    internal sealed partial class LegacyGameVersionJsonSerializerContext : JsonSerializerContext;
}
