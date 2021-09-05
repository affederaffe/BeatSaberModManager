using System;

using BeatSaberModManager.Services.Interfaces;

using Microsoft.Win32;


namespace BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars
{
    public class WindowsProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        private const string kProviderName = nameof(BeatSaberModManager);

        public bool IsProtocolHandlerRegistered(string protocol)
        {
            if (!OperatingSystem.IsWindows()) return false;
            using RegistryKey? protocolKey = Registry.CurrentUser.OpenSubKey(@"software\classes")?.OpenSubKey(protocol);
            string? protocolHandler = protocolKey?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty)?.ToString()?.Split(' ')[0];
            return protocolHandler?[1..^1] == Environment.ProcessPath;
        }

        public void RegisterProtocolHandler(string protocol)
        {
            if (!OperatingSystem.IsWindows()) return;
            using RegistryKey protocolKey = Registry.CurrentUser.OpenSubKey(@"software\classes")?.OpenSubKey(protocol, true) ??
                                            Registry.CurrentUser.CreateSubKey(@"software\classes").CreateSubKey(protocol, true);
            using RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command", true);
            protocolKey.SetValue(string.Empty, $"URL:{protocol} Protocol", RegistryValueKind.String);
            protocolKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
            protocolKey.SetValue("OneClick-Provider", kProviderName, RegistryValueKind.String);
            commandKey.SetValue(string.Empty, $"\"{Environment.ProcessPath}\" \"--install\" \"%1\"");
        }

        public void UnregisterProtocolHandler(string protocol)
        {
            if (!OperatingSystem.IsWindows()) return;
            using RegistryKey? providerKey = Registry.CurrentUser.OpenSubKey(@"software\classes")?.OpenSubKey(protocol);
            string? registeredProviderName = providerKey?.GetValue("OneClick-Provider")?.ToString();
            if (registeredProviderName != kProviderName) return;
            Registry.CurrentUser.DeleteSubKeyTree($@"software\classes\{protocol}", false);
        }
    }
}