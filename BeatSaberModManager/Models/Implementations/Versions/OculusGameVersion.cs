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

        /// <inheritdoc />
        public int CompareTo(IGameVersion? other) => throw new NotImplementedException();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(OculusGameVersion left, OculusGameVersion right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(OculusGameVersion left, OculusGameVersion right)
        {
            return !(left == right);
        }

        public static bool operator <(OculusGameVersion left, OculusGameVersion right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(OculusGameVersion left, OculusGameVersion right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(OculusGameVersion left, OculusGameVersion right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(OculusGameVersion left, OculusGameVersion right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
