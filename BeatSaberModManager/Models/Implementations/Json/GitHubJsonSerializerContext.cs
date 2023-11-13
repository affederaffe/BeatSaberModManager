using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.GitHub;


namespace BeatSaberModManager.Models.Implementations.Json
{
    [JsonSerializable(typeof(Asset))]
    [JsonSerializable(typeof(Release))]
    internal sealed partial class GitHubJsonSerializerContext : JsonSerializerContext;
}
