using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber
{
    /// <inheritdoc />
    public class ModelSaberAssetProvider(ModelSaberModelInstaller modelSaberModelInstaller) : IAssetProvider
    {
        /// <inheritdoc />
        public string Protocol => "modelsaber";

        /// <inheritdoc />
        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => modelSaberModelInstaller.InstallModelAsync(installDir, uri, progress);
    }
}
