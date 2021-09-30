using System.IO;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberInstallDirValidator : IInstallDirValidator
    {
        public bool ValidateInstallDir(string? path) =>
            !string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, "Beat Saber.exe"));

        public string DetectVrPlatform(string path)
        {
            string dataPluginsPath = Path.Combine(path, "Beat Saber_Data", "Plugins");
            string file1 = Path.Combine(dataPluginsPath, "steam_api64.dll");
            string file2 = Path.Combine(dataPluginsPath, "x86_64", "steam_api64.dll");
            return File.Exists(file1) || File.Exists(file2) ? "steam" : "oculus";
        }
    }
}