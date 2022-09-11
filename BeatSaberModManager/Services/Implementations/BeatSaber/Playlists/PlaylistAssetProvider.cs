using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    /// <inheritdoc />
    public class PlaylistAssetProvider : IAssetProvider
    {
        private readonly PlaylistInstaller _playlistInstaller;

        /// <inheritdoc />
        public string Protocol => "bsplaylist";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistAssetProvider"/> class.
        /// </summary>
        public PlaylistAssetProvider(PlaylistInstaller playlistInstaller)
        {
            _playlistInstaller = playlistInstaller;
        }

        /// <inheritdoc />
        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => _playlistInstaller.InstallPlaylistAsync(installDir, new Uri(uri.LocalPath[1..]), progress);
    }
}
