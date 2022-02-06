using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.GitHub;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <inheritdoc />
    [JsonSerializable(typeof(Asset))]
    [JsonSerializable(typeof(Release))]
    internal partial class GitHubJsonSerializerContext : JsonSerializerContext { }
}