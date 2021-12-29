using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModInstaller
    {
        IAsyncEnumerable<IMod> InstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null);
        IAsyncEnumerable<IMod> UninstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null);
        void RemoveAllMods(string installDir);
    }
}