using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModInstaller
    {
        Task<bool> InstallModAsync(IMod modToInstall);
        Task<bool> UninstallModAsync(IMod modToUninstall);
        void RemoveAllMods();
    }
}