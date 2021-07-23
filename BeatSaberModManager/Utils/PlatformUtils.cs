using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace BeatSaberModManager.Utils
{
    public static class PlatformUtils
    {
        public static async Task OpenBrowserOrFileExplorer(string uri)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                await Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true })!.WaitForExitAsync();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                await Process.Start("xdg-open", $"\"{uri}\"")!.WaitForExitAsync();
        }
    }
}