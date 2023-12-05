using System;


namespace BeatSaberModManager.Models.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameVersion : IComparable<IGameVersion>
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

        /// <summary>
        /// 
        /// </summary>
        string? InstallDir { get; set; }
    }
}
