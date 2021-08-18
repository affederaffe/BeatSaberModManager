using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Win32;


namespace BeatSaberModManager.Utils
{
    public static class PlatformUtils
    {
        public static void OpenBrowserOrFileExplorer(string uri)
        {
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            else if (OperatingSystem.IsLinux())
                Process.Start("xdg-open", $"\"{uri}\"");
            else
                throw new PlatformNotSupportedException();
        }

        public static bool IsProtocolHandlerRegistered(string protocol, string providerName) =>
            OperatingSystem.IsWindows()
                ? IsWindowsProtocolHandlerRegistered(protocol)
                : OperatingSystem.IsLinux()
                    ? IsLinuxProtocolHandlerRegistered(protocol, providerName)
                    : throw new PlatformNotSupportedException();

        public static void RegisterProtocolHandler(string protocol, string providerName)
        {
            if (OperatingSystem.IsWindows())
                RegisterWindowsProtocolHandler(protocol, providerName);
            else if (OperatingSystem.IsLinux())
                RegisterLinuxProtocolHandler(protocol, providerName);
            else
                throw new PlatformNotSupportedException();
        }

        public static void UnregisterProtocolHandler(string protocol, string providerName)
        {
            if (OperatingSystem.IsWindows())
                UnregisterWindowsProtocolHandler(protocol, providerName);
            else if (OperatingSystem.IsLinux())
                UnregisterLinuxProtocolHandler(protocol, providerName);
            else
                throw new PlatformNotSupportedException();
        }

        private static void RegisterWindowsProtocolHandler(string protocol, string providerName)
        {
            if (!OperatingSystem.IsWindows()) return;
            using RegistryKey protocolKey = Registry.CurrentUser.OpenSubKey(@"software\classes")?.OpenSubKey(protocol, true) ??
                                            Registry.CurrentUser.CreateSubKey(@"software\classes").CreateSubKey(protocol, true);
            using RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command", true);
            protocolKey.SetValue(string.Empty, $"URL:{protocol} Protocol", RegistryValueKind.String);
            protocolKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
            protocolKey.SetValue("OneClick-Provider", providerName, RegistryValueKind.String);
            commandKey.SetValue(string.Empty, $"\"{Environment.ProcessPath}\" \"--install\" \"%1\"");
        }

        private static void UnregisterWindowsProtocolHandler(string protocol, string providerName)
        {
            if (!OperatingSystem.IsWindows()) return;
            using RegistryKey? providerKey = Registry.CurrentUser.OpenSubKey(@"software\classes")?.OpenSubKey(protocol);
            string? registeredProviderName = providerKey?.GetValue("OneClick-Provider")?.ToString();
            if (registeredProviderName != providerName) return;
            Registry.CurrentUser.DeleteSubKeyTree($@"software\classes\{protocol}", false);
        }

        private static bool IsWindowsProtocolHandlerRegistered(string protocol)
        {
            if (!OperatingSystem.IsWindows()) return false;
            using RegistryKey? protocolKey = Registry.CurrentUser.OpenSubKey(@"software\classes")?.OpenSubKey(protocol);
            string? protocolHandler = protocolKey?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty)?.ToString()?.Split(' ')[0];
            return protocolHandler?[1..^1] == Environment.ProcessPath;
        }

        private static void RegisterLinuxProtocolHandler(string protocol, string providerName)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string handlerDesktopFileName = providerName + ".desktop";
            string handlerDesktopFilePath = Path.Combine(localAppDataPath, "applications", handlerDesktopFileName);
            if (!File.Exists(handlerDesktopFilePath))
            {
                File.WriteAllText(handlerDesktopFilePath, $"[Desktop Entry]\nType=Application\nCategories=Utility;\nName={providerName}\nExec={Environment.ProcessPath} --install %u\nType=Application\nTerminal=false\nMimeType=x-scheme-handler/{protocol};");
                return;
            }

            IEnumerable<string> lines = File.ReadAllLines(handlerDesktopFilePath).Select(x => x.StartsWith("MimeType", StringComparison.Ordinal) ? $"{x}x-scheme-handler/{protocol}" : x);
            File.WriteAllLines(handlerDesktopFilePath, lines);
            Process.Start("xdg-mime", $"\"default\" \"{handlerDesktopFileName}\" \"x-scheme-handler/{protocol}\"").WaitForExit();
        }

        private static void UnregisterLinuxProtocolHandler(string protocol, string providerName)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string localApplicationsPath = Path.Combine(localAppDataPath, "applications");
            string handlerDesktopFilePath = Path.Combine(localApplicationsPath, providerName + ".desktop");
            if (!File.Exists(handlerDesktopFilePath)) return;
            IEnumerable<string> lines = File.ReadAllLines(handlerDesktopFilePath).Select(x => x.StartsWith("MimeType", StringComparison.Ordinal) ? x.Replace($"x-scheme-handler/{protocol};", string.Empty) : x);
            File.WriteAllLines(handlerDesktopFilePath, lines);
            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string mimeappsListFilePath = Path.Combine(configPath, "mimeapps.list");
            if (!File.Exists(mimeappsListFilePath)) return;
            string target = $"x-scheme-handler/{protocol}={providerName}.desktop";
            lines = File.ReadAllLines(mimeappsListFilePath).Where(x => x != target);
            File.WriteAllLines(mimeappsListFilePath, lines);
        }

        private static bool IsLinuxProtocolHandlerRegistered(string protocol, string providerName)
        {
            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string mimeappsListFilePath = Path.Combine(configPath, "mimeapps.list");
            if (!File.Exists(mimeappsListFilePath)) return false;
            string target = $"x-scheme-handler/{protocol}={providerName}.desktop";
            using FileStream stream = File.OpenRead(mimeappsListFilePath);
            using StreamReader reader = new(stream);
            while (stream.Position < stream.Length)
                if (reader.ReadLine() == target) return true;
            return false;
        }
    }
}