using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModInstaller
    {
        IAsyncEnumerable<IMod> InstallModsAsync(IEnumerable<IMod> mods);
        IAsyncEnumerable<IMod> UninstallModsAsync(IEnumerable<IMod> mods);
        void RemoveAllMods();
    }
}