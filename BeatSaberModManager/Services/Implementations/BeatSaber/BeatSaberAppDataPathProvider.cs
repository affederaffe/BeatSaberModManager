using System;
using System.IO;
using System.Runtime.Versioning;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberAppDataPathProvider : IAppDataPathProvider
    {
        /// <inheritdoc />
        public string GetAppDataPath(string installDir) =>
            OperatingSystem.IsWindows() ? GetWindowsAppDataPath()
                : OperatingSystem.IsLinux() ? GetLinuxAppDataPath(installDir)
                    : throw new PlatformNotSupportedException();

        [SupportedOSPlatform("windows")]
        private static string GetWindowsAppDataPath() => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Hyperbolic Magnetism");

        [SupportedOSPlatform("linux")]
        private static string GetLinuxAppDataPath(string installDir) => Path.Join(installDir, "../../compatdata/620980/pfx/drive_c/users/steamuser/AppData/LocalLow/Hyperbolic Magnetism");
    }
}