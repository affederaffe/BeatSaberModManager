using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModInstaller : IModInstaller
    {
        private readonly Settings _settings;
        private readonly IModProvider _modProvider;
        private readonly IHashProvider _hashProvider;

        public BeatModsModInstaller(Settings settings, IModProvider modProvider, IHashProvider hashProvider)
        {
            _settings = settings;
            _modProvider = modProvider;
            _hashProvider = hashProvider;
        }

        public async Task<bool> InstallModAsync(IMod modToInstall)
        {
            string pendingDirPath = Path.Combine(_settings.InstallDir!, "IPA", "Pending");
            if (!Directory.Exists(pendingDirPath)) Directory.CreateDirectory(pendingDirPath);
            IDownload? download = GetDownloadForVRPlatform(modToInstall);
            if (download is null) return false;
            using ZipArchive? archive = await _modProvider.DownloadModAsync(download.Url!);
            if (archive is null || !ValidateDownload(download, archive)) return false;
            if (modToInstall.Name?.ToLowerInvariant() != _modProvider.ModLoaderName)
            {
                archive.ExtractToDirectory(pendingDirPath, true);
                return true;
            }

            archive.ExtractToDirectory(_settings.InstallDir!, true);
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? await InstallBSIPAWindowsAsync()
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && InstallBSIPALinux();
        }

        public async Task<bool> UninstallModAsync(IMod modToUninstall)
        {
            if (modToUninstall.Name?.ToLowerInvariant() == _modProvider.ModLoaderName)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return await UninstallBSIPAWindowsAsync(modToUninstall);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return UninstallBSIPALinux(modToUninstall);
            }

            string pendingDirPath = Path.Combine(_settings.InstallDir!, "IPA", "Pending");
            IDownload? download = GetDownloadForVRPlatform(modToUninstall);
            if (download?.Hashes is null) return false;
            foreach (IHash hash in download.Hashes)
            {
                string pendingPath = Path.Combine(pendingDirPath, hash.File!);
                string normalPath = Path.Combine(_settings.InstallDir!, hash.File!);
                if (File.Exists(pendingPath)) File.Delete(pendingPath);
                if (File.Exists(normalPath)) File.Delete(normalPath);
            }

            return true;
        }

        public void RemoveAllMods()
        {
            string pluginsDirPath = Path.Combine(_settings.InstallDir!, "Plugins");
            string libsDirPath = Path.Combine(_settings.InstallDir!, "Libs");
            string ipaDirPath = Path.Combine(_settings.InstallDir!, "IPA");
            if (Directory.Exists(pluginsDirPath)) Directory.Delete(pluginsDirPath, true);
            if (Directory.Exists(libsDirPath)) Directory.Delete(libsDirPath, true);
            if (Directory.Exists(ipaDirPath)) Directory.Delete(ipaDirPath, true);
        }

        private bool ValidateDownload(IDownload download, ZipArchive archive)
        {
            foreach (IHash hash in download.Hashes!)
            {
                using Stream? stream = archive.GetEntry(hash.File!)?.Open();
                if (stream is null) return false;
                string strHash = _hashProvider.CalculateHashForStream(stream);
                if (strHash != hash.Hash) return false;
            }

            return true;
        }

        private async Task<bool> InstallBSIPAWindowsAsync()
        {
            string winhttpPath = Path.Combine(_settings.InstallDir!, "winhttp.dll");
            string bsipaPath = Path.Combine(_settings.InstallDir!, "IPA.exe");
            if (File.Exists(winhttpPath) || !File.Exists(bsipaPath)) return false;
            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                Arguments = "-n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return false;
            await process.WaitForExitAsync();
            return true;
        }

        private bool InstallBSIPALinux()
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_settings.InstallDir!);
            IPA.Program.Main(new[] { "-n", "-f", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            string protonPrefixPath = Path.Combine($"{_settings.InstallDir}/../..", "compatdata/620980/pfx/user.reg");
            if (!File.Exists(protonPrefixPath)) return false;
            string[] lines = File.ReadAllLines(protonPrefixPath);
            using StreamWriter streamWriter = File.AppendText(protonPrefixPath);
            if (!lines.Contains("[Software\\\\Wine\\\\DllOverrides]"))
                streamWriter.WriteLine("[Software\\\\Wine\\\\DllOverrides]");
            if (!lines.Contains("\"winhttp\"=\"native,builtin\""))
                streamWriter.WriteLine("\"winhttp\"=\"native,builtin\"");
            return true;
        }

        private async Task<bool> UninstallBSIPAWindowsAsync(IMod bsipa)
        {
            string bsipaPath = Path.Combine(_settings.InstallDir!, "IPA.exe");
            if (File.Exists(bsipaPath))
            {
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = bsipaPath,
                    Arguments = "--revert -n"
                };

                Process? process = Process.Start(processStartInfo);
                if (process is null) return false;
                await process.WaitForExitAsync();
            }

            IDownload? download = GetDownloadForVRPlatform(bsipa);
            if (download?.Hashes is null) return false;
            foreach (IHash hash in download.Hashes)
            {
                string fileName = hash.File!.Replace("IPA/", "").Replace("Data", "Beat Saber_Data");
                string filePath = Path.Combine(_settings.InstallDir!, fileName);
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            return true;
        }

        private bool UninstallBSIPALinux(IMod bsipa)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_settings.InstallDir!);
            IPA.Program.Main(new[] { "--revert", "-n", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            IDownload? download = GetDownloadForVRPlatform(bsipa);
            if (download?.Hashes is null) return false;
            foreach (IHash hash in download.Hashes)
            {
                string fileName = hash.File!.Replace("IPA/", "").Replace("Data", "Beat Saber_Data");
                string filePath = Path.Combine(_settings.InstallDir!, fileName);
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            return true;
        }

        private IDownload? GetDownloadForVRPlatform(IMod mod)
            => mod.Downloads?.FirstOrDefault(x => x.Type!.ToLowerInvariant() == "universal" || x.Type == _settings.VRPlatform!);
    }
}