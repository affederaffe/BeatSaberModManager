using System;

using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverAssetProvider : IAssetProvider
    {
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public BeatSaverAssetProvider(BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public string Protocol => "beatsaver";

        public async Task<bool> InstallAssetAsync(Uri uri) => await _beatSaverMapInstaller.InstallBeatSaverMapAsync(uri.Host).ConfigureAwait(false);
    }
}