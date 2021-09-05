using System;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IAssetProvider
    {
        string Protocol { get; }
        Task<bool> InstallAssetAsync(Uri uri, IStatusProgress? progress = null);
    }
}