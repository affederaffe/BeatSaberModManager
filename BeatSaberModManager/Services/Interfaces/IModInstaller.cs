using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModInstaller
    {
        IAsyncEnumerable<IMod> InstallModsAsync(string installDir, IEnumerable<IMod> mods);
        IAsyncEnumerable<IMod> UninstallModsAsync(string installDir, IEnumerable<IMod> mods);
        void RemoveAllMods(string installDir);
    }
}