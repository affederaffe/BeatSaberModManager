using System;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.Versions
{
    /// <summary>
    /// TODO
    /// </summary>
    public class SteamGameVersion : IGameVersion
    {
        /// <inheritdoc />
        public override int GetHashCode() => ManifestId.GetHashCode();

        /// <inheritdoc />
        [JsonPropertyName("BSVersion")]
        public required string GameVersion { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("ReleaseDate")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime ReleaseDate { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("ReleaseURL")]
        public Uri? ReleaseUrl { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("ReleaseImg")]
        public Uri? ReleaseImage { get; init; }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonPropertyName("BSManifest")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong ManifestId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public string? InstallDir { get; set; }

        [JsonIgnore]
        private Version Version
        {
            get
            {
                if (_version is not null)
                    return _version;
                ReadOnlySpan<char> span = GameVersion.AsSpan();
                int end = span.LastIndexOf('.') + 1;
                while (end < span.Length && char.IsNumber(span[end]))
                    end++;
                span = span[..end];
                return !Version.TryParse(span, out _version)
                    ? throw new InvalidOperationException($"Game version has the wrong format: {GameVersion}")
                    : _version;
            }
        }

        private Version? _version;

        /// <inheritdoc />
        public int CompareTo(IGameVersion? other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (other is not SteamGameVersion steamLegacyGameVersion)
                return 1;
            return Version.CompareTo(steamLegacyGameVersion.Version);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is not SteamGameVersion steamLegacyGameVersion)
                return false;
            return Version == steamLegacyGameVersion.Version;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(SteamGameVersion? left, SteamGameVersion? right) => left?.Equals(right) ?? right is null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(SteamGameVersion left, SteamGameVersion right) => !(left == right);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(SteamGameVersion? left, SteamGameVersion? right) => left is null ? right is not null : left.CompareTo(right) < 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(SteamGameVersion? left, SteamGameVersion? right) => left is null || left.CompareTo(right) <= 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(SteamGameVersion? left, SteamGameVersion? right) => left is not null && left.CompareTo(right) > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(SteamGameVersion? left, SteamGameVersion? right) => left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
