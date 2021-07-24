using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IModProvider
    {
        string ModLoaderName { get; }
        IMod[]? AvailableMods { get; }
        HashSet<IMod>? InstalledMods { get; }
        Dictionary<IMod, HashSet<IMod>> Dependencies { get; }
        void ResolveDependencies(IMod modToResolveFor);
        void UnresolveDependencies(IMod modToUnresolveFor);
        Task LoadInstalledModsAsync();
        Task LoadAvailableModsForVersionAsync(string version);
        Task<ZipArchive?> DownloadModAsync(string url);
    }
}