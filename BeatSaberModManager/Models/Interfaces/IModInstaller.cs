using System.Threading.Tasks;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IModInstaller
    {
        Task<bool> InstallModAsync(IMod modToInstall);
        Task<bool> UninstallModAsync(IMod modToUninstall);
    }
}