using System;
using System.Threading.Tasks;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IAssetProvider
    {
        string Protocol { get; }
        Task<bool> InstallAssetAsync(Uri uri, IStatusProgress? progress = null);
    }
}