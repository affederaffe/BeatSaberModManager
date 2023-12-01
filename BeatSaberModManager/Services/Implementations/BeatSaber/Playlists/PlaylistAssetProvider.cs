using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    /// <inheritdoc />
    public class PlaylistAssetProvider(PlaylistInstaller playlistInstaller) : IAssetProvider
    {
        /// <inheritdoc />
        public string Protocol => "bsplaylist";

        /// <inheritdoc />
        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
        {
            ArgumentNullException.ThrowIfNull(uri);
            return playlistInstaller.InstallPlaylistAsync(installDir, new Uri(uri.PathAndQuery[1..]), progress);
        }
    }
}
