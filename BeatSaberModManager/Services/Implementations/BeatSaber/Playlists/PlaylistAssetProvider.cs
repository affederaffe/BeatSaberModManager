﻿using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.Playlists
{
    public class PlaylistAssetProvider : IAssetProvider
    {
        private readonly PlaylistInstaller _playlistInstaller;

        public string Protocol => "bsplaylist";

        public PlaylistAssetProvider(PlaylistInstaller playlistInstaller)
        {
            _playlistInstaller = playlistInstaller;
        }

        public async Task<bool> InstallAssetAsync(Uri uri, IStatusProgress? progress = null)
            => await _playlistInstaller.InstallPlaylistAsync(new Uri(uri.AbsolutePath[1..]), progress).ConfigureAwait(false);
    }
}