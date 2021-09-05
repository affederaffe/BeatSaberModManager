using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.Options;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModInstaller : IModInstaller
    {
        private readonly SettingsStore _settingsStore;
        private readonly IModProvider _modProvider;
        private readonly IHashProvider _hashProvider;

        public BeatModsModInstaller(IOptions<SettingsStore> settingsStore, IModProvider modProvider, IHashProvider hashProvider)
        {
            _settingsStore = settingsStore.Value;
            _modProvider = modProvider;
            _hashProvider = hashProvider;
        }

        public async Task<bool> InstallModAsync(IMod modToInstall)
        {
            if (modToInstall is not BeatModsMod beatModsMod || !Directory.Exists(_settingsStore.InstallDir)) return false;
            string pendingDirPath = Path.Combine(_settingsStore.InstallDir, "IPA", "Pending");
            if (!Directory.Exists(pendingDirPath)) Directory.CreateDirectory(pendingDirPath);
            BeatModsDownload download = beatModsMod.GetDownloadForVRPlatform(_settingsStore.VRPlatform!);
            using ZipArchive? archive = await _modProvider.DownloadModAsync(download.Url).ConfigureAwait(false);
            if (archive is null || !ValidateDownload(download, archive)) return false;
            if (beatModsMod.Name.ToLowerInvariant() != _modProvider.ModLoaderName)
            {
                archive.ExtractToDirectory(pendingDirPath, true);
                _modProvider.InstalledMods?.Add(modToInstall);
                return true;
            }

            archive.ExtractToDirectory(_settingsStore.InstallDir, true);
            if (!await InstallBSIPAAsync().ConfigureAwait(false)) return false;
            _modProvider.InstalledMods?.Add(modToInstall);
            return true;
        }

        public async Task<bool> UninstallModAsync(IMod modToUninstall)
        {
            if (modToUninstall is not BeatModsMod beatModsMod || !Directory.Exists(_settingsStore.InstallDir)) return false;
            if (modToUninstall.Name.ToLowerInvariant() == _modProvider.ModLoaderName) return await UninstallBSIPAAsync(beatModsMod).ConfigureAwait(false);
            string pendingDirPath = Path.Combine(_settingsStore.InstallDir, "IPA", "Pending");
            BeatModsDownload download = beatModsMod.GetDownloadForVRPlatform(_settingsStore.VRPlatform!);
            foreach (BeatModsHash hash in download.Hashes)
            {
                string pendingPath = Path.Combine(pendingDirPath, hash.File);
                string normalPath = Path.Combine(_settingsStore.InstallDir, hash.File);
                if (File.Exists(pendingPath)) File.Delete(pendingPath);
                if (File.Exists(normalPath)) File.Delete(normalPath);
            }

            _modProvider.InstalledMods?.Remove(modToUninstall);
            return true;
        }

        public void RemoveAllMods()
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return;
            string pluginsDirPath = Path.Combine(_settingsStore.InstallDir, "Plugins");
            string libsDirPath = Path.Combine(_settingsStore.InstallDir, "Libs");
            string ipaDirPath = Path.Combine(_settingsStore.InstallDir, "IPA");
            string winhttpPath = Path.Combine(_settingsStore.InstallDir, "winhttp.dll");
            if (Directory.Exists(pluginsDirPath)) Directory.Delete(pluginsDirPath, true);
            if (Directory.Exists(libsDirPath)) Directory.Delete(libsDirPath, true);
            if (Directory.Exists(ipaDirPath)) Directory.Delete(ipaDirPath, true);
            if (File.Exists(winhttpPath)) File.Delete(winhttpPath);
        }

        private bool ValidateDownload(BeatModsDownload download, ZipArchive archive)
        {
            foreach (BeatModsHash hash in download.Hashes)
            {
                using Stream? stream = archive.GetEntry(hash.File)?.Open();
                if (stream is null) return false;
                string strHash = _hashProvider.CalculateHashForStream(stream);
                if (strHash != hash.Hash) return false;
            }

            return true;
        }

        private async Task<bool> InstallBSIPAAsync() =>
            OperatingSystem.IsWindows()
                ? await InstallBSIPAWindowsAsync().ConfigureAwait(false)
                : OperatingSystem.IsLinux()
                    ? await InstallBSIPALinux().ConfigureAwait(false)
                    : throw new PlatformNotSupportedException();

        private async Task<bool> UninstallBSIPAAsync(BeatModsMod bsipa) =>
            OperatingSystem.IsWindows()
                ? await UninstallBSIPAWindowsAsync(bsipa).ConfigureAwait(false)
                : OperatingSystem.IsLinux()
                    ? UninstallBSIPALinux(bsipa)
                    : throw new PlatformNotSupportedException();

        private async Task<bool> InstallBSIPAWindowsAsync()
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            string winhttpPath = Path.Combine(_settingsStore.InstallDir, "winhttp.dll");
            string bsipaPath = Path.Combine(_settingsStore.InstallDir, "IPA.exe");
            if (File.Exists(winhttpPath) || !File.Exists(bsipaPath)) return false;
            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = _settingsStore.InstallDir,
                Arguments = "-n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return false;
            await process.WaitForExitAsync().ConfigureAwait(false);
            return true;
        }

        private async Task<bool> InstallBSIPALinux()
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_settingsStore.InstallDir);
            IPA.Program.Main(new[] { "-n", "-f", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            string protonPrefixPath = Path.Combine($"{_settingsStore.InstallDir}/../..", "compatdata/620980/pfx/user.reg");
            if (!File.Exists(protonPrefixPath)) return false;
            string[] lines = await File.ReadAllLinesAsync(protonPrefixPath);
            await using StreamWriter streamWriter = File.AppendText(protonPrefixPath);
            if (!lines.Contains("[Software\\\\Wine\\\\DllOverrides]"))
                await streamWriter.WriteLineAsync("[Software\\\\Wine\\\\DllOverrides]");
            if (!lines.Contains("\"winhttp\"=\"native,builtin\""))
                await streamWriter.WriteLineAsync("\"winhttp\"=\"native,builtin\"");
            return true;
        }

        private async Task<bool> UninstallBSIPAWindowsAsync(BeatModsMod bsipa)
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            string bsipaPath = Path.Combine(_settingsStore.InstallDir, "IPA.exe");
            if (!File.Exists(bsipaPath)) return TryRemoveBSIPAFiles(bsipa);
            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = _settingsStore.InstallDir,
                Arguments = "--revert -n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return false;
            await process.WaitForExitAsync().ConfigureAwait(false);
            return TryRemoveBSIPAFiles(bsipa);
        }

        private bool UninstallBSIPALinux(BeatModsMod bsipa)
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_settingsStore.InstallDir);
            IPA.Program.Main(new[] { "--revert", "-n", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            return TryRemoveBSIPAFiles(bsipa);
        }

        private bool TryRemoveBSIPAFiles(BeatModsMod bsipa)
        {
            BeatModsDownload download = bsipa.GetDownloadForVRPlatform(_settingsStore.VRPlatform!);
            foreach (BeatModsHash hash in download.Hashes)
            {
                string fileName = hash.File.Replace("IPA/", string.Empty).Replace("Data", "Beat Saber_Data");
                string filePath = Path.Combine(_settingsStore.InstallDir!, fileName);
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            _modProvider.InstalledMods?.Remove(bsipa);
            return true;
        }
    }
}