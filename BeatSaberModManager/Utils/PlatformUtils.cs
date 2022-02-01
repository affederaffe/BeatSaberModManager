using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace BeatSaberModManager.Utils
{
    public static class PlatformUtils
    {
        public static bool TryOpenUri(string uri) =>
            OperatingSystem.IsWindows()
                ? TryStartProcess(new ProcessStartInfo(uri) { UseShellExecute = true }, out _)
                : OperatingSystem.IsLinux() && TryStartProcess(new ProcessStartInfo("xdg-open", $"\"{uri}\""), out _);

        public static bool TryStartProcess(ProcessStartInfo startInfo, [MaybeNullWhen(false)] out Process process)
        {
            process = null;
            try
            {
                process = Process.Start(startInfo);
                return process is not null;
            }
            catch (InvalidOperationException) { }
            catch (PlatformNotSupportedException) { }
            return false;
        }
    }
}