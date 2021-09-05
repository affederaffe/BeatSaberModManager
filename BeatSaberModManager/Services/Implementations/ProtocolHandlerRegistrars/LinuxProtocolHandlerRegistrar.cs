using System;
using System.Diagnostics;
using System.IO;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars
{
    public class LinuxProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        private readonly string _localAppDataPath;

        private const string kProviderName = nameof(BeatSaberModManager);

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
            File.WriteAllText(handlerPath, $"[Desktop Entry]\nName={kProviderName}\nComment=URL:{protocol} Protocol\nType=Application\nCategories=Utility\nExec={Environment.ProcessPath} --install %u\nTerminal=false\nNoDisplay=true\nMimeType=x-scheme-handler/{protocol}");
            Process.Start("xdg-mime", $"\"default\" \"{handlerName}\" \"x-scheme-handler/{protocol}\"");
        }

        public void UnregisterProtocolHandler(string protocol)
        {
            string handlerPath = GetHandlerPathForProtocol(protocol);
            if (!File.Exists(handlerPath)) return;
            File.Delete(handlerPath);
        }

        private string GetHandlerPathForProtocol(string protocol) => Path.Combine(_localAppDataPath, GetHandlerNameForProtocol(protocol));

        private static string GetHandlerNameForProtocol(string protocol) => $"{kProviderName}-url-{protocol}.desktop";
    }
}