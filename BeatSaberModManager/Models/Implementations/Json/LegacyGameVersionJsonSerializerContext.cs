using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.LegacyVersions;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(SteamLegacyGameVersion[]))]
    internal sealed partial class LegacyGameVersionJsonSerializerContext : JsonSerializerContext;
}
