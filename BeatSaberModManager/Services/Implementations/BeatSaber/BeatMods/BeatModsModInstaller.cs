using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class BeatModsModInstaller : IModInstaller
    {
        private readonly IModProvider _modProvider;
        private readonly IStatusProgress _progress;

        public BeatModsModInstaller(IModProvider modProvider, IStatusProgress progress)
        {
            _modProvider = modProvider;
            _progress = progress;
        }

        public async IAsyncEnumerable<IMod> InstallModsAsync(string installDir, IEnumerable<IMod> mods)
        {
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            IEnumerable<string> urls = beatModsMods.Select(x => x.Downloads.First().Url);
            string pendingDirPath = Path.Combine(installDir, "IPA", "Pending");
            IOUtils.TryCreateDirectory(pendingDirPath);
            int i = 0;
            _progress.Report(beatModsMods[i].Name);
            _progress.Report(ProgressBarStatusType.Installing);
            await foreach (ZipArchive archive in _modProvider.DownloadModsAsync(urls, _progress).ConfigureAwait(false))
            {
                bool isModLoader = _modProvider.IsModLoader(beatModsMods[i]);
                string extractDir = isModLoader ? installDir : pendingDirPath;
                IOUtils.TryExtractArchive(archive, extractDir, true);
                if (isModLoader) await InstallBsipaAsync(installDir).ConfigureAwait(false);
                _modProvider.InstalledMods?.Add(beatModsMods[i]);
                yield return beatModsMods[i++];
                if (i >= beatModsMods.Length) break;
                _progress.Report(beatModsMods[i].Name);
            }

            _progress.Report(string.Empty);
            _progress.Report(ProgressBarStatusType.Completed);
        }

        public async IAsyncEnumerable<IMod> UninstallModsAsync(string installDir, IEnumerable<IMod> mods)
        {
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            _progress.Report(ProgressBarStatusType.Uninstalling);
            for (int i = 0; i < beatModsMods.Length; i++)
            {
                _progress.Report(beatModsMods[i].Name);
                _progress.Report(((double)i + 1) / beatModsMods.Length);
                bool isModLoader = _modProvider.IsModLoader(beatModsMods[i]);
                if (isModLoader) await UninstallBsipaAsync(installDir, beatModsMods[i]).ConfigureAwait(false);
                else RemoveModFiles(installDir, beatModsMods[i]);
                _modProvider.InstalledMods?.Remove(beatModsMods[i]);
                yield return beatModsMods[i];
            }

            _progress.Report(string.Empty);
            _progress.Report(ProgressBarStatusType.Completed);
        }

        public void RemoveAllMods(string installDir)
        {
            string pluginsDirPath = Path.Combine(installDir, "Plugins");
            string libsDirPath = Path.Combine(installDir, "Libs");
            string ipaDirPath = Path.Combine(installDir, "IPA");
            string winhttpPath = Path.Combine(installDir, "winhttp.dll");
            IOUtils.TryDeleteDirectory(pluginsDirPath, true);
            IOUtils.TryDeleteDirectory(libsDirPath, true);
            IOUtils.TryDeleteDirectory(ipaDirPath, true);
            IOUtils.TryDeleteFile(winhttpPath);
        }

        private static Task InstallBsipaAsync(string installDir) =>
            OperatingSystem.IsWindows()
                ? InstallBsipaWindowsAsync(installDir)
                : OperatingSystem.IsLinux()
                    ? InstallBsipaLinux(installDir)
                    : throw new PlatformNotSupportedException();

        private static ValueTask UninstallBsipaAsync(string installDir, BeatModsMod bsipa) =>
            OperatingSystem.IsWindows()
                ? UninstallBsipaWindowsAsync(installDir, bsipa)
                : OperatingSystem.IsLinux()
                    ? UninstallBsipaLinux(installDir, bsipa)
                    : throw new PlatformNotSupportedException();

        [SupportedOSPlatform("windows")]
        private static async Task InstallBsipaWindowsAsync(string installDir)
        {
            string winhttpPath = Path.Combine(installDir, "winhttp.dll");
            string bsipaPath = Path.Combine(installDir, "IPA.exe");
            if (File.Exists(winhttpPath) || !File.Exists(bsipaPath)) return;
            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = installDir,
                Arguments = "-n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        [SupportedOSPlatform("linux")]
        private static async Task InstallBsipaLinux(string installDir)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(new[] { "-n", "-f", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            string protonRegPath = Path.Combine($"{installDir}/../..", "compatdata/620980/pfx/user.reg");
            if (!IOUtils.TryOpenFile(protonRegPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, FileOptions.Asynchronous, out FileStream? fileStream)) return;
            await using FileStream fs = fileStream;
            using StreamReader reader = new(fs);
            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
            await using StreamWriter streamWriter = new(fs);
            if (!content.Contains("[Software\\\\Wine\\\\DllOverrides]\n\"winhttp\"=\"native,builtin\""))
                await streamWriter.WriteLineAsync("\n[Software\\\\Wine\\\\DllOverrides]\n\"winhttp\"=\"native,builtin\"").ConfigureAwait(false);
        }

        [SupportedOSPlatform("windows")]
        private static async ValueTask UninstallBsipaWindowsAsync(string installDir, BeatModsMod bsipa)
        {
            string bsipaPath = Path.Combine(installDir, "IPA.exe");
            if (!File.Exists(bsipaPath))
            {
                RemoveBsipaFiles(installDir, bsipa);
                return;
            }

            ProcessStartInfo processStartInfo = new()
            {
                FileName = bsipaPath,
                WorkingDirectory = installDir,
                Arguments = "--revert -n"
            };

            Process? process = Process.Start(processStartInfo);
            if (process is null) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
            RemoveBsipaFiles(installDir, bsipa);
        }

        [SupportedOSPlatform("linux")]
        private static ValueTask UninstallBsipaLinux(string installDir, BeatModsMod bsipa)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(new[] { "--revert", "-n", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            RemoveBsipaFiles(installDir, bsipa);
            return new ValueTask();
        }

        private static void RemoveBsipaFiles(string installDir, BeatModsMod bsipa)
        {
            foreach (BeatModsHash hash in bsipa.Downloads.First().Hashes)
            {
                string fileName = hash.File.Replace("IPA/Data", "Beat Saber_Data");
                string path = Path.Combine(installDir, fileName);
                IOUtils.TryDeleteFile(path);
            }
        }

        private static void RemoveModFiles(string installDir, BeatModsMod mod)
        {
            string pendingDirPath = Path.Combine(installDir, "IPA", "Pending");
            BeatModsDownload download = mod.Downloads.First();
            foreach (BeatModsHash hash in download.Hashes)
            {
                string pendingPath = Path.Combine(pendingDirPath, hash.File);
                string normalPath = Path.Combine(installDir, hash.File);
                IOUtils.TryDeleteFile(pendingPath);
                IOUtils.TryDeleteFile(normalPath);
            }
        }
    }
}