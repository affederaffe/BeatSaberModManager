using System;
using System.Runtime.Versioning;

using BeatSaberModManager.Services.Interfaces;

using Microsoft.Win32;


namespace BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars
{
    /// <inheritdoc />
    [SupportedOSPlatform("windows")]
    public class WindowsProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        /// <inheritdoc />
        public bool IsProtocolHandlerRegistered(string protocol)
        {
            RegistryKey? protocolKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Classes")?.OpenSubKey(protocol);
            string? commandValue = protocolKey?.OpenSubKey("shell")?.OpenSubKey("open")?.OpenSubKey("command")?.GetValue(string.Empty)?.ToString();
            if (commandValue is null || Environment.ProcessPath is null) return false;
            int end = Environment.ProcessPath.Length + 1;
            if (commandValue.Length < end) return false;
            return commandValue[1..end] == Environment.ProcessPath;
        }

        /// <inheritdoc />
        public void RegisterProtocolHandler(string protocol)
        {
            using RegistryKey protocolKey = Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("Classes").CreateSubKey(protocol, true);
            using RegistryKey commandKey = protocolKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
            protocolKey.SetValue(string.Empty, $"URL:{protocol} Protocol", RegistryValueKind.String);
            protocolKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
            protocolKey.SetValue("OneClick-Provider", ThisAssembly.Info.Product, RegistryValueKind.String);
            commandKey.SetValue(string.Empty, $"\"{Environment.ProcessPath}\" \"--install\" \"%1\"");
        }

        /// <inheritdoc />
        public void UnregisterProtocolHandler(string protocol)
        {
            using RegistryKey? protocolKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Classes")?.OpenSubKey(protocol, true);
            string? registeredProviderName = protocolKey?.GetValue("OneClick-Provider")?.ToString();
            if (registeredProviderName != ThisAssembly.Info.Product) return;
            protocolKey?.DeleteSubKeyTree(string.Empty, false);
        }
    }
}