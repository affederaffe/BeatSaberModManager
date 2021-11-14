using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModProvider
    {
        string ModLoaderName { get; }
        IMod[]? AvailableMods { get; }
        HashSet<IMod>? InstalledMods { get; }
        IEnumerable<IMod> GetDependencies(IMod mod);
        Task LoadInstalledModsAsync();
        Task LoadAvailableModsForCurrentVersionAsync();
        Task LoadAvailableModsForVersionAsync(string version);
        IAsyncEnumerable<ZipArchive?> DownloadModsAsync(IEnumerable<string> urls, IProgress<double>? progress = null);
    }
}