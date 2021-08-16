using System.Threading.Tasks;


namespace BeatSaberModManager.Models.Implementations.Interfaces
{
    public interface IModInstaller
    {
        Task<bool> InstallModAsync(IMod modToInstall);
        Task<bool> UninstallModAsync(IMod modToUninstall);
        void RemoveAllMods();
    }
}