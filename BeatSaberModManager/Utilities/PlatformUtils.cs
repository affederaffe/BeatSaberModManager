using System;
using System.Diagnostics;


namespace BeatSaberModManager.Utilities
{
    public static class PlatformUtils
    {
        public static void OpenUri(string uri)
        {
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            else if (OperatingSystem.IsLinux())
                Process.Start("xdg-open", $"\"{uri}\"");
            else
                throw new PlatformNotSupportedException();
        }
    }
}