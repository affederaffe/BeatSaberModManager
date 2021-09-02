using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.ProtocolHandlerRegistrars
{
    public class LinuxProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        private readonly string _mimeAppsListFilePath;
        private readonly string _handlerDesktopFileName;
        private readonly string _handlerDesktopFilePath;

        private const string kProviderName = nameof(BeatSaberModManager);

        public LinuxProtocolHandlerRegistrar()
        {
            string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _mimeAppsListFilePath = Path.Combine(configPath, "mimeapps.list");
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _handlerDesktopFileName = kProviderName + ".desktop";
            _handlerDesktopFilePath = Path.Combine(localAppDataPath, "applications", _handlerDesktopFileName);
        }

        public bool IsProtocolHandlerRegistered(string protocol)
        {
            if (!File.Exists(_mimeAppsListFilePath)) return false;
            string target = $"x-scheme-handler/{protocol}={_handlerDesktopFileName}";
            return File.ReadLines(_mimeAppsListFilePath).Any(x => x == target);
        }

        public void RegisterProtocolHandler(string protocol)
        {
            if (!File.Exists(_handlerDesktopFilePath))
            {
                File.WriteAllText(_handlerDesktopFilePath, $"[Desktop Entry]\nType=Application\nCategories=Utility;\nName=URL:{protocol} Protocol\nExec={Environment.ProcessPath} --install %u\nType=Application\nTerminal=false\nNoDisplay=true\nMimeType=x-scheme-handler/{protocol};");
                return;
            }

            IEnumerable<string> lines = File.ReadLines(_handlerDesktopFilePath).Select(x => x.StartsWith("MimeType", StringComparison.Ordinal) ? $"{x}x-scheme-handler/{protocol}" : x);
            File.WriteAllLines(_handlerDesktopFilePath, lines);
            Process.Start("xdg-mime", $"\"default\" \"{_handlerDesktopFileName}\" \"x-scheme-handler/{protocol}\"");
        }

        public void UnregisterProtocolHandler(string protocol)
        {
            if (!File.Exists(_handlerDesktopFilePath)) return;
            IEnumerable<string> lines = File.ReadLines(_handlerDesktopFilePath).Select(x => x.StartsWith("MimeType", StringComparison.Ordinal) ? x.Replace($"x-scheme-handler/{protocol};", string.Empty) : x);
            File.WriteAllLines(_handlerDesktopFilePath, lines);
            if (!File.Exists(_mimeAppsListFilePath)) return;
            string target = $"x-scheme-handler/{protocol}={_handlerDesktopFileName}";
            lines = File.ReadLines(_mimeAppsListFilePath).Where(x => x != target);
            File.WriteAllLines(_mimeAppsListFilePath, lines);
        }
    }
}