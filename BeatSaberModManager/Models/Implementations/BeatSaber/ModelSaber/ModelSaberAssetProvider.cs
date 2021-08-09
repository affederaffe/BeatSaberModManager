using System;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberAssetProvider : IAssetProvider
    {
        private readonly ModelSaberModelInstaller _modelSaberModelInstaller;

        public ModelSaberAssetProvider(ModelSaberModelInstaller modelSaberModelInstaller)
        {
            _modelSaberModelInstaller = modelSaberModelInstaller;
        }

        public string Protocol => "modelsaber";

        public async Task<bool> InstallAssetAsync(Uri uri) => await _modelSaberModelInstaller.InstallModelAsync(uri);
    }
}