using System;


namespace BeatSaberModManager.Models.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILegacyGameVersion : IComparable<ILegacyGameVersion>
    {
        /// <summary>
        /// 
        /// </summary>
        string GameVersion { get; }

        /// <summary>
        /// 
        /// </summary>
        DateTime ReleaseDate { get; }

        /// <summary>
        /// 
        /// </summary>
        Uri? ReleaseUrl { get; }

        /// <summary>
        /// 
        /// </summary>
        Uri? ReleaseImage { get; }
    }
}
