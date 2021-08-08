using System;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.Playlist
{
    public class PlaylistAssetProvider : IAssetProvider
    {
        private readonly PlaylistInstaller _playlistInstaller;

        public string Protocol => "bsplaylist";

        public PlaylistAssetProvider(PlaylistInstaller playlistInstaller)
        {
            _playlistInstaller = playlistInstaller;
        }

        public async Task<bool> InstallAssetAsync(Uri uri) => await _playlistInstaller.InstallPlaylistFromUrlAsync(uri.AbsolutePath[1..]);
    }
}