using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars
{
    [SupportedOSPlatform("linux")]
    public class LinuxProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        private readonly string _localAppDataPath;

        public LinuxProtocolHandlerRegistrar()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _localAppDataPath = Path.Combine(appDataPath, "applications");
        }

        public bool IsProtocolHandlerRegistered(string protocol) => File.Exists(GetHandlerPathForProtocol(protocol));

        public void RegisterProtocolHandler(string protocol)
        {
            string handlerName = GetHandlerNameForProtocol(protocol);
            string handlerPath = Path.Combine(_localAppDataPath, handlerName);
            File.WriteAllText(handlerPath, $"[Desktop Entry]\nName={ThisAssembly.Info.Product}\nComment=URL:{protocol} Protocol\nType=Application\nCategories=Utility\nExec={Environment.ProcessPath} --install %u\nTerminal=false\nNoDisplay=true\nMimeType=x-scheme-handler/{protocol}");
            PlatformUtils.TryStartProcess(new ProcessStartInfo("xdg-mime", $"\"default\" \"{handlerName}\" \"x-scheme-handler/{protocol}\""), out _);
        }

        public void UnregisterProtocolHandler(string protocol)
        {
            string handlerPath = GetHandlerPathForProtocol(protocol);
            IOUtils.TryDeleteFile(handlerPath);
        }

        private string GetHandlerPathForProtocol(string protocol) => Path.Combine(_localAppDataPath, GetHandlerNameForProtocol(protocol));

        private static string GetHandlerNameForProtocol(string protocol) => $"{ThisAssembly.Info.Product}-url-{protocol}.desktop";
    }
}