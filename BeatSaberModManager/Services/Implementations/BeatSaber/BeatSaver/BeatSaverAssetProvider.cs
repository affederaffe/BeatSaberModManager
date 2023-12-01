using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatSaver
{
    /// <inheritdoc />
    public class BeatSaverAssetProvider(BeatSaverMapInstaller beatSaverMapInstaller) : IAssetProvider
    {
        /// <inheritdoc />
        public string Protocol => "beatsaver";

        /// <inheritdoc />
        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
        {
            ArgumentNullException.ThrowIfNull(uri);
            return beatSaverMapInstaller.InstallBeatSaverMapByKeyAsync(installDir, uri.Host, progress);
        }
    }
}
