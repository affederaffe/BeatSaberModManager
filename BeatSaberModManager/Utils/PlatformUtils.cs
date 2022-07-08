using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace BeatSaberModManager.Utils
{
    /// <summary>
    /// Utilities for platform specific operations.
    /// </summary>
    public static class PlatformUtils
    {
        /// <summary>
        /// Attempts to use the standard program to open the <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The uri to open.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public static bool TryOpenUri(string uri) =>
            OperatingSystem.IsWindows()
                ? TryStartProcess(new ProcessStartInfo(uri) { UseShellExecute = true }, out _)
                : OperatingSystem.IsLinux() && TryStartProcess(new ProcessStartInfo("xdg-open", $"\"{uri}\""), out _);

        /// <summary>
        /// Attempts to start a new process.
        /// </summary>
        /// <param name="startInfo">The information used to start the process.</param>
        /// <param name="process">The <see cref="Process"/> when the operation succeeds.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public static bool TryStartProcess(ProcessStartInfo startInfo, [MaybeNullWhen(false)] out Process process)
        {
            try
            {
                process = Process.Start(startInfo);
                return process is not null;
            }
            catch (InvalidOperationException) { }
            catch (PlatformNotSupportedException) { }

            process = null;
            return false;
        }
    }
}
