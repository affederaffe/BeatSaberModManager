using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver
{
    public class BeatSaverAssetProvider : IAssetProvider
    {
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        public BeatSaverAssetProvider(BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        public string Protocol => "beatsaver";

        public async Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => await _beatSaverMapInstaller.InstallBeatSaverMapAsync(installDir, uri.Host, progress).ConfigureAwait(false);
    }
}