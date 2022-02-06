using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <inheritdoc />
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(Dictionary<string, string[]>))]
    internal partial class CommonJsonSerializerContext : JsonSerializerContext { }
}