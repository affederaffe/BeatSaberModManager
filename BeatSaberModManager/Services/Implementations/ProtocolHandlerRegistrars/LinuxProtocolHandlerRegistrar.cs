using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.ProtocolHandlerRegistrars
{
    /// <inheritdoc />
    [SupportedOSPlatform("linux")]
    public class LinuxProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        private readonly object _lock;
        private readonly string _localAppDataPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinuxProtocolHandlerRegistrar"/> class.
        /// </summary>
        public LinuxProtocolHandlerRegistrar()
        {
            _lock = new object();
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _localAppDataPath = Path.Join(appDataPath, "applications");
        }

        /// <inheritdoc />
        public bool IsProtocolHandlerRegistered(string protocol)
        {
            string handlerPath = GetHandlerPathForProtocol(protocol);
            return IOUtils.TryReadAllLines(handlerPath, out string[]? text) && text.Any(static s => s.StartsWith($"Exec={Environment.ProcessPath}", StringComparison.Ordinal));
        }

        /// <inheritdoc />
        public void RegisterProtocolHandler(string protocol)
        {
            string handlerName = GetHandlerNameForProtocol(protocol);
            string handlerPath = Path.Join(_localAppDataPath, handlerName);
            File.WriteAllText(handlerPath, GetDesktopFileContent(protocol));
            lock (_lock)
            {
                if (PlatformUtils.TryStartProcess(new ProcessStartInfo("xdg-mime", $"\"default\" \"{handlerName}\" \"x-scheme-handler/{protocol}\""), out Process? process))
                    process.WaitForExit();
            }
        }

        /// <inheritdoc />
        public void UnregisterProtocolHandler(string protocol)
        {
            string handlerPath = GetHandlerPathForProtocol(protocol);
            IOUtils.TryDeleteFile(handlerPath);
        }

        private string GetHandlerPathForProtocol(string protocol) => Path.Join(_localAppDataPath, GetHandlerNameForProtocol(protocol));

        private static string GetHandlerNameForProtocol(string protocol) => $"{ThisAssembly.Info.Product}-url-{protocol}.desktop";

        private static string GetDesktopFileContent(string protocol) =>
            @$"[Desktop Entry]
Name={ThisAssembly.Info.Product}
Comment=URL:{protocol} Protocol
Type=Application
Categories=Utility
Exec={Environment.ProcessPath} --install %u
Terminal=false
NoDisplay=true
MimeType=x-scheme-handler/{protocol}
";
    }
}
