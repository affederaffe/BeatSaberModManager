using System;
using System.IO;
using System.Runtime.Versioning;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberGamePathsProvider : IGamePathsProvider
    {
        /// <inheritdoc />
        public string GetAppDataPath(IGameVersion gameVersion)
        {
            ArgumentNullException.ThrowIfNull(gameVersion);
            ArgumentNullException.ThrowIfNull(gameVersion.InstallDir);
            if (OperatingSystem.IsWindows())
                return GetWindowsAppDataPath();
            if (OperatingSystem.IsLinux())
                return GetLinuxAppDataPath(gameVersion.InstallDir);
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        public string GetLogsPath(IGameVersion gameVersion)
        {
            ArgumentNullException.ThrowIfNull(gameVersion);
            return Path.Join(gameVersion.InstallDir, "Logs");
        }

        [SupportedOSPlatform("windows")]
        private static string GetWindowsAppDataPath() => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Hyperbolic Magnetism");

        [SupportedOSPlatform("linux")]
        private static string GetLinuxAppDataPath(string installDir) => Path.Join(installDir, "../../compatdata/620980/pfx/drive_c/users/steamuser/AppData/LocalLow/Hyperbolic Magnetism");
    }
}
