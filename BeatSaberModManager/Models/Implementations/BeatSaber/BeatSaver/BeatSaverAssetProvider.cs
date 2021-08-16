﻿using System;

using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Interfaces;


namespace BeatSaberModManager.Models.Implementations.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverAssetProvider : IAssetProvider
    {
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public BeatSaverAssetProvider(BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public string Protocol => "beatsaver";

        public async Task<bool> InstallAssetAsync(Uri uri, IStatusProgress? progress = null)
            => await _beatSaverMapInstaller.InstallBeatSaverMapAsync(uri.Host, progress).ConfigureAwait(false);
    }
}