using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModInstaller : IModInstaller
    {
        private readonly AppSettings _appSettings;
        private readonly IModProvider _modProvider;
        private readonly IStatusProgress _progress;

        public BeatModsModInstaller(ISettings<AppSettings> appSettings, IModProvider modProvider, IStatusProgress progress)
        {
            _appSettings = appSettings.Value;
            _modProvider = modProvider;
            _progress = progress;
        }

        public async IAsyncEnumerable<IMod> InstallModsAsync(IEnumerable<IMod> mods)
        {
            if (!Directory.Exists(_appSettings.InstallDir)) yield break;
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            IEnumerable<string> urls = beatModsMods.Select(x => x.GetDownloadForVRPlatform(_appSettings.VRPlatform!).Url);
            string pendingDirPath = Path.Combine(_appSettings.InstallDir, "IPA", "Pending");
            if (!Directory.Exists(pendingDirPath)) Directory.CreateDirectory(pendingDirPath);
            int i = 0;
            _progress.Report(beatModsMods[i].Name);
            _progress.Report(ProgressBarStatusType.Installing);
            await foreach (ZipArchive? archive in _modProvider.DownloadModsAsync(urls, _progress))
            {
                if (beatModsMods[i].Name.ToLowerInvariant() == _modProvider.ModLoaderName)
                {
                    archive?.ExtractToDirectory(_appSettings.InstallDir, true);
                    await InstallBSIPAAsync().ConfigureAwait(false);
                }
                else
                {
                    archive?.ExtractToDirectory(pendingDirPath, true);
                }

                _modProvider.InstalledMods?.Add(beatModsMods[i]);
                yield return beatModsMods[i++];
                if (i >= beatModsMods.Length) yield break;
                _progress.Report(beatModsMods[i].Name);
            }
        }

        public async IAsyncEnumerable<IMod> UninstallModsAsync(IEnumerable<IMod> mods)
        {
            if (!Directory.Exists(_appSettings.InstallDir)) yield break;
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            _progress.Report(ProgressBarStatusType.Uninstalling);
            for (int i = 0; i < beatModsMods.Length; i++)
            {
                _progress.Report(beatModsMods[i].Name);
                _progress.Report(((double)i + 1) / beatModsMods.Length);
                if (beatModsMods[i].Name.ToLowerInvariant() == _modProvider.ModLoaderName)
                {
                    await UninstallBSIPAAsync(beatModsMods[i]);
                    continue;
                }

                string pendingDirPath = Path.Combine(_appSettings.InstallDir, "IPA", "Pending");
                BeatModsDownload download = beatModsMods[i].GetDownloadForVRPlatform(_appSettings.VRPlatform!);
                foreach (BeatModsHash hash in download.Hashes)
                {
                    string pendingPath = Path.Combine(pendingDirPath, hash.File);
                    string normalPath = Path.Combine(_appSettings.InstallDir, hash.File);
                    File.Delete(pendingPath);
                    File.Delete(normalPath);
                }

                _modProvider.InstalledMods?.Remove(beatModsMods[i]);
                yield return beatModsMods[i];
            }
        }

        public void RemoveAllMods()
        {
            if (!Directory.Exists(_appSettings.InstallDir)) return;
            string pluginsDirPath = Path.Combine(_appSettings.InstallDir, "Plugins");
            string libsDirPath = Path.Combine(_appSettings.InstallDir, "Libs");
            string ipaDirPath = Path.Combine(_appSettings.InstallDir, "IPA");
            string winhttpPath = Path.Combine(_appSettings.InstallDir, "winhttp.dll");
            if (Directory.Exists(pluginsDirPath)) Directory.Delete(pluginsDirPath, true);
            if (Directory.Exists(libsDirPath)) Directory.Delete(libsDirPath, true);
            if (Directory.Exists(ipaDirPath)) Directory.Delete(ipaDirPath, true);
            File.Delete(winhttpPath);
        }

        private Task InstallBSIPAAsync() =>
            OperatingSystem.IsWindows()
                ? InstallBSIPAWindowsAsync()
                : OperatingSystem.IsLinux()
                    ? InstallBSIPALinux()
                    : throw new PlatformNotSupportedException();

        private Task UninstallBSIPAAsync(BeatModsMod bsipa) =>
            OperatingSystem.IsWindows()
                ? UninstallBSIPAWindowsAsync(bsipa)
                : OperatingSystem.IsLinux()
                    ? UninstallBSIPALinux(bsipa)
                    : throw new PlatformNotSupportedException();

        private async Task InstallBSIPAWindowsAsync()
        {
            if (!Directory.Exists(_appSettings.InstallDir)) return;
            string winhttpPath = Path.Combine(_appSettings.InstallDir, "winhttp.dll");
            string bsipaPath = Path.Combine(_appSettings.InstallDir, "IPA.exe");
            if (File.Exists(winhttpPath) || !File.Exists(bsipaPath)) return;
            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = _appSettings.InstallDir,
                Arguments = "-n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        private async Task InstallBSIPALinux()
        {
            if (!Directory.Exists(_appSettings.InstallDir)) return;
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_appSettings.InstallDir);
            IPA.Program.Main(new[] { "-n", "-f", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            string protonPrefixPath = Path.Combine($"{_appSettings.InstallDir}/../..", "compatdata/620980/pfx/user.reg");
            if (!File.Exists(protonPrefixPath)) return;
            string[] lines = await File.ReadAllLinesAsync(protonPrefixPath);
            await using StreamWriter streamWriter = File.AppendText(protonPrefixPath);
            if (!lines.Contains("[Software\\\\Wine\\\\DllOverrides]"))
                await streamWriter.WriteLineAsync("[Software\\\\Wine\\\\DllOverrides]");
            if (!lines.Contains("\"winhttp\"=\"native,builtin\""))
                await streamWriter.WriteLineAsync("\"winhttp\"=\"native,builtin\"");
        }

        private async Task UninstallBSIPAWindowsAsync(BeatModsMod bsipa)
        {
            if (!Directory.Exists(_appSettings.InstallDir)) return;
            string bsipaPath = Path.Combine(_appSettings.InstallDir, "IPA.exe");
            if (!File.Exists(bsipaPath))
            {
                RemoveBSIPAFiles(bsipa);
                return;
            }

            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = _appSettings.InstallDir,
                Arguments = "--revert -n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
            RemoveBSIPAFiles(bsipa);
        }

        private Task UninstallBSIPALinux(BeatModsMod bsipa)
        {
            if (!Directory.Exists(_appSettings.InstallDir)) return Task.CompletedTask;
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_appSettings.InstallDir);
            IPA.Program.Main(new[] { "--revert", "-n", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            RemoveBSIPAFiles(bsipa);
            return Task.CompletedTask;
        }

        private void RemoveBSIPAFiles(BeatModsMod bsipa)
        {
            BeatModsDownload download = bsipa.GetDownloadForVRPlatform(_appSettings.VRPlatform!);
            foreach (BeatModsHash hash in download.Hashes)
            {
                string fileName = hash.File.Replace("IPA/", string.Empty).Replace("Data", "Beat Saber_Data");
                string filePath = Path.Combine(_appSettings.InstallDir!, fileName);
                File.Delete(filePath);
            }

            _modProvider.InstalledMods?.Remove(bsipa);
        }
    }
}