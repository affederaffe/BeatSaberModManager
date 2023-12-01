using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Json
{
    /// <summary>
    /// TODO
    /// </summary>
    public class UnixDateTimeConverter : JsonConverter<DateTime>
    {
        /// <inheritdoc />
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetString() is not { } value)
                return default;
            long t = long.Parse(value, NumberFormatInfo.InvariantInfo);
            return DateTime.UnixEpoch.AddSeconds(t);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);
            writer.WriteNumberValue(value.Subtract(DateTime.UnixEpoch).Seconds);
        }
    }
}
