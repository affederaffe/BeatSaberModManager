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
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc />
    public class BeatModsModInstaller(IModProvider modProvider) : IModInstaller
    {
        private static readonly string[] _installIpaArgs = { "-n", "-f", "--relativeToPwd", "Beat Saber.exe" };
        private static readonly string[] _uninstallIpaArgs = { "--revert", "-n", "--relativeToPwd", "Beat Saber.exe" };

        /// <inheritdoc />
        public async Task<bool> InstallModAsync(string installDir, IMod modification)
        {
            if (modification is not BeatModsMod beatModsMod)
                return false;
            string pendingDirPath = Path.Join(installDir, "IPA", "Pending");
            if (!IOUtils.TryCreateDirectory(pendingDirPath))
                return false;
            using ZipArchive? archive = await modProvider.DownloadModAsync(beatModsMod).ConfigureAwait(false);
            if (archive is null)
                return false;
            bool isModLoader = modProvider.IsModLoader(beatModsMod);
            string extractDir = isModLoader ? installDir : pendingDirPath;
            bool success = IOUtils.TryExtractArchive(archive, extractDir, true);
            return isModLoader ? success && await InstallBsipaAsync(installDir).ConfigureAwait(false) : success;
        }

        /// <inheritdoc />
        public async Task UninstallModAsync(string installDir, IMod modification)
        {
            if (modification is not BeatModsMod beatModsMod)
                return;
            bool isModLoader = modProvider.IsModLoader(beatModsMod);
            if (isModLoader)
                await UninstallBsipaAsync(installDir, beatModsMod).ConfigureAwait(false);
            else RemoveModFiles(installDir, beatModsMod);
        }

        /// <inheritdoc />
        public void RemoveAllModFiles(string installDir)
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

        private static Task<bool> InstallBsipaAsync(string installDir) =>
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
        private static async Task<bool> InstallBsipaWindowsAsync(string installDir)
        {
            string winhttpPath = Path.Join(installDir, "winhttp.dll");
            if (File.Exists(winhttpPath))
                return true;

            ProcessStartInfo processStartInfo = new()
            {
                FileName = Path.Join(installDir, "IPA.exe"),
                WorkingDirectory = installDir,
                Arguments = "-n"
            };

            if (!PlatformUtils.TryStartProcess(processStartInfo, out Process? process))
                return false;
            await process.WaitForExitAsync().ConfigureAwait(false);
            return true;
        }

        [SupportedOSPlatform("linux")]
        private static async Task<bool> InstallBsipaLinuxAsync(string installDir)
        {
            string protonRegPath = Path.Join(installDir, "../../compatdata/620980/pfx/user.reg");
            string[]? lines = await IOUtils.TryReadAllLinesAsync(protonRegPath).ConfigureAwait(false);
            if (lines is null)
                return false;
            IEnumerable<string> newLines = lines.Select(static x => x.StartsWith(@"[Software\\Wine\\DllOverrides]", StringComparison.Ordinal) ? x + "\n\"winhttp\"=\"native,builtin\"" : x);
            await File.WriteAllLinesAsync(protonRegPath, newLines).ConfigureAwait(false);
            string winhttpPath = Path.Join(installDir, "winhttp.dll");
            if (File.Exists(winhttpPath))
                return true;
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(installDir);
            IPA.Program.Main(_installIpaArgs);
            Directory.SetCurrentDirectory(oldDir);
            return true;
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
            IPA.Program.Main(_uninstallIpaArgs);
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
