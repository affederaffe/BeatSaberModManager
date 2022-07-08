using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <inheritdoc />
    public class VersionConverter : JsonConverter<Version>
    {
        /// <inheritdoc />
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Version.TryParse(reader.GetString() ?? string.Empty, out Version? version) ? version : new Version(0, 0, 0);

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
