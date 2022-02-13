using System;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Defines a method to install additional assets like maps, models or playlists.
    /// </summary>
    public interface IAssetProvider
    {
        /// <summary>
        /// The protocol that the specific implementation handles.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Asynchronously downloads and installs an asset.
        /// </summary>
        /// <param name="installDir">The game's installation directory.</param>
        /// <param name="uri">The <see cref="Uri"/> to download the asset from.<br/>
        /// The <see cref="Uri.Scheme"/> has to match <see cref="Protocol"/>.</param>
        /// <param name="progress">Optionally track the progress of the operation.</param>
        /// <returns>True when the installation succeeded, false otherwise.</returns>
        Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null);
    }
}