using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModProvider
    {
        IReadOnlyList<IMod>? AvailableMods { get; }
        HashSet<IMod>? InstalledMods { get; }
        bool IsModLoader(IMod? mod);
        IEnumerable<IMod> GetDependencies(IMod mod);
        Task LoadInstalledModsAsync(string installDir);
        Task LoadAvailableModsForCurrentVersionAsync(string installDir);
        Task LoadAvailableModsForVersionAsync(string version);
        IAsyncEnumerable<ZipArchive> DownloadModsAsync(IEnumerable<string> urls, IProgress<double>? progress = null);
    }
}