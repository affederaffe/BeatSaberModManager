using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    public class PlaylistAssetProvider : IAssetProvider
    {
        private readonly PlaylistInstaller _playlistInstaller;

        public string Protocol => Constants.PlaylistProtocol;

        public PlaylistAssetProvider(PlaylistInstaller playlistInstaller)
        {
            _playlistInstaller = playlistInstaller;
        }

        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => _playlistInstaller.InstallPlaylistAsync(installDir, new Uri(uri.AbsolutePath[1..]), progress);
    }
}