using System;
using System.Text.Json.Serialization;

using BeatSaberModManager.Models.Implementations.Json;
using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.LegacyVersions
{
    /// <summary>
    /// TODO
    /// </summary>
    public class SteamLegacyGameVersion : ILegacyGameVersion
    {
        /// <inheritdoc />
        public override int GetHashCode() => ManifestId.GetHashCode();

        /// <inheritdoc />
        [JsonPropertyName("BSVersion")]
        public required string GameVersion { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("ReleaseDate")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public required DateTime ReleaseDate { get; init; }

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
        public required ulong ManifestId { get; init; }

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
        public int CompareTo(ILegacyGameVersion? other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (other is not SteamLegacyGameVersion steamLegacyGameVersion)
                return 1;
            return Version.CompareTo(steamLegacyGameVersion.Version);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is not SteamLegacyGameVersion steamLegacyGameVersion)
                return false;
            return ManifestId == steamLegacyGameVersion.ManifestId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(SteamLegacyGameVersion? left, SteamLegacyGameVersion? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(SteamLegacyGameVersion left, SteamLegacyGameVersion right) => !(left == right);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(SteamLegacyGameVersion? left, SteamLegacyGameVersion? right)
        {
            if (left is null)
                return right is not null;
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(SteamLegacyGameVersion? left, SteamLegacyGameVersion? right) => left is null || left.CompareTo(right) <= 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(SteamLegacyGameVersion? left, SteamLegacyGameVersion? right) => left is not null && left.CompareTo(right) > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(SteamLegacyGameVersion? left, SteamLegacyGameVersion? right) => left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
