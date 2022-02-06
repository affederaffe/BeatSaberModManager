using System;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber
{
    /// <inheritdoc />
    public class ModelSaberAssetProvider : IAssetProvider
    {
        private readonly ModelSaberModelInstaller _modelSaberModelInstaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSaberAssetProvider"/> class.
        /// </summary>
        public ModelSaberAssetProvider(ModelSaberModelInstaller modelSaberModelInstaller)
        {
            _modelSaberModelInstaller = modelSaberModelInstaller;
        }

        /// <inheritdoc />
        public string Protocol => "modelsaber";

        /// <inheritdoc />
        public Task<bool> InstallAssetAsync(string installDir, Uri uri, IStatusProgress? progress = null)
            => _modelSaberModelInstaller.InstallModelAsync(installDir, uri, progress);
    }
}