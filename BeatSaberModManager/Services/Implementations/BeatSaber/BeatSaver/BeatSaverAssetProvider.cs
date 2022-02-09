using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver
{
    /// <inheritdoc />
    public class BeatSaverAssetProvider : IAssetProvider
    {
        private readonly BeatSaverMapInstaller _beatSaverMapInstaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeatSaverAssetProvider"/> class.
        /// </summary>
        public BeatSaverAssetProvider(BeatSaverMapInstaller beatSaverMapInstaller)
        {
            _beatSaverMapInstaller = beatSaverMapInstaller;
        }

        /// <inheritdoc />
        public string Protocol => "beatsaver";

        /// <inheritdoc />
        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => _beatSaverMapInstaller.InstallBeatSaverMapByKeyAsync(installDir, uri.Host, progress);
    }
}