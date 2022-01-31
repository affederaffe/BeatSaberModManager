using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IInstallDirLocator
    {
        ValueTask<InstallDirLocatorResult> LocateInstallDirAsync();
    }
}