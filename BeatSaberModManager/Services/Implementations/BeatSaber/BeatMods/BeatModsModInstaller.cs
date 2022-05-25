using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc />
    public class BeatModsModInstaller : IModInstaller
    {
        private readonly IModProvider _modProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeatModsModInstaller"/> class.
        /// </summary>
        public BeatModsModInstaller(IModProvider modProvider)
        {
            _modProvider = modProvider;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IMod> InstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null)
        {
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            string pendingDirPath = Path.Join(installDir, "IPA", "Pending");
            IOUtils.TryCreateDirectory(pendingDirPath);
            int i = 0;
            progress?.Report(new ProgressInfo(StatusType.Installing, beatModsMods[i].Name));
            await foreach (ZipArchive archive in _modProvider.DownloadModsAsync(beatModsMods, progress).ConfigureAwait(false))
            {
                bool isModLoader = _modProvider.IsModLoader(beatModsMods[i]);
                string extractDir = isModLoader ? installDir : pendingDirPath;
                if (!IOUtils.TryExtractArchive(archive, extractDir, true)) continue;
                if (isModLoader) await InstallBsipaAsync(installDir).ConfigureAwait(false);
                yield return beatModsMods[i++];
                if (i >= beatModsMods.Length) break;
                progress?.Report(new ProgressInfo(StatusType.Installing, beatModsMods[i].Name));
            }

            progress?.Report(new ProgressInfo(StatusType.Completed, null));
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IMod> UninstallModsAsync(string installDir, IEnumerable<IMod> mods, IStatusProgress? progress = null)
        {
            BeatModsMod[] beatModsMods = mods.OfType<BeatModsMod>().ToArray();
            if (beatModsMods.Length <= 0) yield break;
            for (int i = 0; i < beatModsMods.Length; i++)
            {
                progress?.Report(new ProgressInfo(StatusType.Uninstalling, beatModsMods[i].Name));
                progress?.Report(((double)i + 1) / beatModsMods.Length);
                bool isModLoader = _modProvider.IsModLoader(beatModsMods[i]);
                if (isModLoader) await UninstallBsipaAsync(installDir, beatModsMods[i]).ConfigureAwait(false);
                else RemoveModFiles(installDir, beatModsMods[i]);
                yield return beatModsMods[i];
            }

            progress?.Report(new ProgressInfo(StatusType.Completed, null));
        }

        /// <inheritdoc />
        public void RemoveAllMods(string installDir)
        {
            string pluginsDirPath = Path.Join(installDir, "Plugins");
            string libsDirPath = Path.Join(installDir, "Libs");
            string ipaDirPath = Path.Join(installDir, "IPA");
            string winhttpPath = Path.Join(installDir, "winhttp.dll");
            IOUtils.TryDeleteDirectory(pluginsDirPath, true);
            IOUtils.TryDeleteDirectory(libsDirPath, true);
            IOUtils.TryDeleteDirectory(ipaDirPath, true);
            IOUtils.TryDeleteFile(winhttpPath);
        }

        private static Task InstallBsipaAsync(string installDir) =>
            OperatingSystem.IsWindows()
                ? InstallBsipaWindowsAsync(installDir)
                : OperatingSystem.IsLinux()
                    ? InstallBsipaLinuxAsync(installDir)
                    : throw new PlatformNotSupportedException();

        private static ValueTask UninstallBsipaAsync(string installDir, BeatModsMod bsipa) =>
            OperatingSystem.IsWindows()
                ? UninstallBsipaWindowsAsync(installDir, bsipa)
                : OperatingSystem.IsLinux()
                    ? UninstallBsipaLinuxAsync(installDir, bsipa)
                    : throw new PlatformNotSupportedException();

        [SupportedOSPlatform("windows")]
        private static async Task InstallBsipaWindowsAsync(string installDir)
        {
            string winhttpPath = Path.Join(installDir, "winhttp.dll");
            if (File.Exists(winhttpPath)) return;

            ProcessStartInfo processStartInfo = new()
            {
                FileName = Path.Join(installDir, "IPA.exe"),
                WorkingDirectory = installDir,
                Arguments = "-n"
            };

            if (!PlatformUtils.TryStartProcess(processStartInfo, out Process? process)) return;
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        [SupportedOSPlatform("linux")]
        private static async Task InstallBsipaLinuxAsync(string installDir)
        {
            string winhttpPath = Path.Join(installDir, "winhttp.dll");
            if (File.Exists(winhttpPath)) return;

            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(new[] { "-n", "-f", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);

            string protonRegPath = Path.Join(installDir, "../../compatdata/620980/pfx/user.reg");
            await using FileStream? fileStream = IOUtils.TryOpenFile(protonRegPath, new FileStreamOptions { Access = FileAccess.ReadWrite, Options = FileOptions.Asynchronous });
            if (fileStream is null) return;
            using StreamReader reader = new(fileStream);
            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
            await using StreamWriter streamWriter = new(fileStream);
            if (!content.Contains("[Software\\\\Wine\\\\DllOverrides]\n\"winhttp\"=\"native,builtin\""))
                await streamWriter.WriteLineAsync("\n[Software\\\\Wine\\\\DllOverrides]\n\"winhttp\"=\"native,builtin\"").ConfigureAwait(false);
        }

        [SupportedOSPlatform("windows")]
        private static async ValueTask UninstallBsipaWindowsAsync(string installDir, BeatModsMod bsipa)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = Path.Join(installDir, "IPA.exe"),
                WorkingDirectory = installDir,
                Arguments = "--revert -n"
            };

            if (PlatformUtils.TryStartProcess(processStartInfo, out Process? process))
                await process.WaitForExitAsync().ConfigureAwait(false);
            RemoveBsipaFiles(installDir, bsipa);
        }

        [SupportedOSPlatform("linux")]
        private static ValueTask UninstallBsipaLinuxAsync(string installDir, BeatModsMod bsipa)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(new[] { "--revert", "-n", "--relativeToPwd", "Beat Saber.exe" });
            Directory.SetCurrentDirectory(oldDir);
            RemoveBsipaFiles(installDir, bsipa);
            return ValueTask.CompletedTask;
        }

        private static void RemoveBsipaFiles(string installDir, BeatModsMod bsipa)
        {
            foreach (BeatModsHash hash in bsipa.Downloads[0].Hashes)
            {
                string fileName = hash.File.Replace("IPA/Data", "Beat Saber_Data", StringComparison.Ordinal).Replace("IPA/", null, StringComparison.Ordinal);
                string path = Path.Join(installDir, fileName);
                IOUtils.TryDeleteFile(path);
            }
        }

        private static void RemoveModFiles(string installDir, BeatModsMod mod)
        {
            string pendingDirPath = Path.Join(installDir, "IPA", "Pending");
            foreach (BeatModsHash hash in mod.Downloads[0].Hashes)
            {
                string pendingPath = Path.Join(pendingDirPath, hash.File);
                string normalPath = Path.Join(installDir, hash.File);
                IOUtils.TryDeleteFile(pendingPath);
                IOUtils.TryDeleteFile(normalPath);
            }
        }
    }
}