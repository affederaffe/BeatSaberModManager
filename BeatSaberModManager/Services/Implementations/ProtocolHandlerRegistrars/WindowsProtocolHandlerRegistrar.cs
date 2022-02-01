using System;
using System.Runtime.Versioning;

using BeatSaberModManager.Services.Interfaces;

using Microsoft.Win32;


namespace BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars
{
    [SupportedOSPlatform("windows")]
    public class WindowsProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        private const string CommandIndicator = @"shell\open\command";

        public bool IsProtocolHandlerRegistered(string protocol)
        {
            using RegistryKey? protocolKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Classes")?.OpenSubKey(protocol);
            string? protocolHandler = protocolKey?.OpenSubKey(CommandIndicator)?.GetValue(string.Empty)?.ToString()?.Split(' ')[0];
            return protocolHandler?[1..^1] == Environment.ProcessPath;
        }

        public void RegisterProtocolHandler(string protocol)
        {
            using RegistryKey protocolKey = Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("Classes").CreateSubKey(protocol, true);
            using RegistryKey commandKey = protocolKey.CreateSubKey(CommandIndicator, true);
            protocolKey.SetValue(string.Empty, $"URL:{protocol} Protocol", RegistryValueKind.String);
            protocolKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
            protocolKey.SetValue("OneClick-Provider", ThisAssembly.Info.Product, RegistryValueKind.String);
            commandKey.SetValue(string.Empty, $"\"{Environment.ProcessPath}\" \"--install\" \"%1\"");
        }

        public void UnregisterProtocolHandler(string protocol)
        {
            using RegistryKey? protocolKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Classes")?.OpenSubKey(protocol);
            string? registeredProviderName = protocolKey?.GetValue("OneClick-Provider")?.ToString();
            if (registeredProviderName != ThisAssembly.Info.Product) return;
            protocolKey?.DeleteSubKeyTree(string.Empty, false);
        }
    }
}