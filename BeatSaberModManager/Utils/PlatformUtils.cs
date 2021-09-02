using System;
using System.Diagnostics;
using System.IO;


namespace BeatSaberModManager.Utils
{
    public static class PlatformUtils
    {
        public static void OpenFolder(string? path)
        {
            if (!Directory.Exists(path)) return;
            OpenBrowserOrFolder(path);
        }

        public static void OpenBrowser(string? url)
        {
            if (url is null) return;
            OpenBrowserOrFolder(url);
        }

        private static void OpenBrowserOrFolder(string uri)
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