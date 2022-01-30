using System.IO;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberInstallDirValidator : IInstallDirValidator
    {
        public bool ValidateInstallDir(string? path) =>
            !string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, Constants.BeatSaberExe));
    }
}