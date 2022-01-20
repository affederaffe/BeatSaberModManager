using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IModProvider
    {
        bool IsModLoader(IMod? modification);
        IEnumerable<IMod> GetDependencies(IMod modification);
        Task<IEnumerable<IMod>?> GetInstalledModsAsync(string installDir);
        Task<IEnumerable<IMod>?> GetAvailableModsForCurrentVersionAsync(string installDir);
        Task<IEnumerable<IMod>?> GetAvailableModsForVersionAsync(string version);
        IAsyncEnumerable<ZipArchive> DownloadModsAsync(IEnumerable<string> urls, IProgress<double>? progress = null);
    }
}