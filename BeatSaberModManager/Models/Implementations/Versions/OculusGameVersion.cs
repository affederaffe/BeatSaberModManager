using System;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.Versions
{
    /// <summary>
    /// 
    /// </summary>
    public class OculusGameVersion : IGameVersion
    {
        /// <inheritdoc />
        public required string GameVersion { get; init; }

        /// <inheritdoc />
        public DateTime ReleaseDate { get; init; }

        /// <inheritdoc />
        public Uri? ReleaseUrl { get; init; }

        /// <inheritdoc />
        public Uri? ReleaseImage { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public string? InstallDir { get; set; }

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
            if (other is not OculusGameVersion oculusGameVersion)
                return 1;
            return Version.CompareTo(oculusGameVersion.Version);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is not OculusGameVersion oculusGameVersion)
                return false;
            return GameVersion == oculusGameVersion.GameVersion;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override int GetHashCode() => GameVersion.GetHashCode(StringComparison.InvariantCulture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(OculusGameVersion? left, OculusGameVersion? right) => left?.Equals(right) ?? right is null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(OculusGameVersion left, OculusGameVersion right) => !(left == right);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(OculusGameVersion? left, OculusGameVersion? right) => left is null ? right is not null : left.CompareTo(right) < 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(OculusGameVersion? left, OculusGameVersion? right) => left is null || left.CompareTo(right) <= 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(OculusGameVersion? left, OculusGameVersion? right) => left is not null && left.CompareTo(right) > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(OculusGameVersion? left, OculusGameVersion? right) => left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
