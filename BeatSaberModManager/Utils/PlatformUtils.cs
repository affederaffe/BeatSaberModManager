using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;


namespace BeatSaberModManager.Utils
{
    public static class PlatformUtils
    {
        private static readonly bool? _isWindowsAdmin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) : null;

        public static async Task OpenBrowserOrFileExplorer(string uri)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                await Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true })!.WaitForExitAsync();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                await Process.Start("xdg-open", $"\"{uri}\"")!.WaitForExitAsync();
        }

        public static bool IsProtocolHandlerRegistered(string protocol, string providerName)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? IsWindowsProtocolHandlerRegistered(protocol, providerName)
                 : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? IsLinuxProtocolHandlerRegistered(protocol, providerName)
                 : throw new PlatformNotSupportedException();
        }

        public static void RegisterProtocolHandler(string protocol, string description, string providerName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                RegisterWindowsProtocolHandler(protocol, description, providerName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                RegisterLinuxProtocolHandler(protocol, description, providerName);
        }

        public static void UnregisterProtocolHandler(string protocol, string providerName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                UnregisterWindowsProtocolHandler(protocol, providerName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                UnregisterLinuxProtocolHandler(protocol, providerName);
        }

        private static void RegisterWindowsProtocolHandler(string protocol, string description, string providerName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IsWindowsProtocolHandlerRegistered(protocol, providerName)) return;
            if (!_isWindowsAdmin.GetValueOrDefault())
            {
                RunProcessAsWindowsAdmin($"\"--register\" \"{protocol}\" \"{description}\" \"{providerName}\"");
                return;
            }

            using Microsoft.Win32.RegistryKey protocolKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(protocol, true) ??
                                                            Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(protocol, true);
            // ReSharper disable once ConstantNullCoalescingCondition
            using Microsoft.Win32.RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command", true) ??
                                                           Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(@"shell\open\command", true);
            if (protocolKey.GetValue("OneClick-Provider")?.ToString() == providerName) return;
            protocolKey.SetValue("", description, Microsoft.Win32.RegistryValueKind.String);
            protocolKey.SetValue("URL Protocol", "", Microsoft.Win32.RegistryValueKind.String);
            protocolKey.SetValue("OneClick-Provider", providerName, Microsoft.Win32.RegistryValueKind.String);
            commandKey.SetValue("", $"\"{Environment.ProcessPath}\" \"--install\" \"%1\"");
        }

        private static void UnregisterWindowsProtocolHandler(string protocol, string providerName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !IsWindowsProtocolHandlerRegistered(protocol, providerName)) return;
            if (!_isWindowsAdmin.GetValueOrDefault())
            {
                RunProcessAsWindowsAdmin($"\"--unregister\" \"{protocol}\" \"{providerName}\"");
                return;
            }

            using Microsoft.Win32.RegistryKey? protocolKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(protocol, true);
            if (protocolKey is not null && protocolKey.GetValue("OneClick-Provider")?.ToString() == providerName)
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(protocol);
        }

        private static bool IsWindowsProtocolHandlerRegistered(string protocol, string providerName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;
            using Microsoft.Win32.RegistryKey? protocolKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(protocol);
            return protocolKey is not null && protocolKey.GetValue("OneClick-Provider")?.ToString() == providerName;
        }

        private static void RunProcessAsWindowsAdmin(string args)
        {
            Process process = new()
            {
                StartInfo =
                {
                    FileName = Environment.ProcessPath,
                    Arguments = args,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static void RegisterLinuxProtocolHandler(string protocol, string description, string providerName)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string localApplicationsPath = Path.Combine(localAppDataPath, "applications");
            string handlerDesktopFileName = providerName + ".desktop";
            string handlerDesktopFilePath = Path.Combine(localApplicationsPath, handlerDesktopFileName);
            if (!File.Exists(handlerDesktopFilePath))
            {
                File.WriteAllText(handlerDesktopFilePath, $"[Desktop Entry]\nType=Application\nName={providerName}\nComment={description}\nExec={Environment.ProcessPath} --install %u\nType=Application\nTerminal=false\nMimeType=x-scheme-handler/{protocol};");
                return;
            }

            string[] lines = File.ReadAllLines(handlerDesktopFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("MimeType", StringComparison.Ordinal)) continue;
                lines[i] = lines[i] + $"x-scheme-handler/{protocol};";
            }

            File.WriteAllLines(handlerDesktopFilePath, lines);
            Process.Start("xdg-mime", $"\"default\" \"{handlerDesktopFileName}\" \"x-scheme-handler/{protocol}\"").WaitForExit();
        }

        private static void UnregisterLinuxProtocolHandler(string protocol, string providerName)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string localApplicationsPath = Path.Combine(localAppDataPath, "applications");
            string handlerDesktopFilePath = Path.Combine(localApplicationsPath, providerName + ".desktop");
            if (!File.Exists(handlerDesktopFilePath)) return;
            string[] lines = File.ReadAllLines(handlerDesktopFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("MimeType", StringComparison.Ordinal)) continue;
                lines[i] = lines[i].Replace($"x-scheme-handler/{protocol};", string.Empty);
            }

            File.WriteAllLines(handlerDesktopFilePath, lines);
            // TODO: Remove mimeapps.list entry
        }

        private static bool IsLinuxProtocolHandlerRegistered(string protocol, string providerName)
        {
            Process process = Process.Start("xdg-mime", $"\"query\" \"default\" \"x-scheme-handler/{protocol}\"");
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return !string.IsNullOrEmpty(output) && output == providerName + ".desktop";
        }
    }
}