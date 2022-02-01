using System.Diagnostics.CodeAnalysis;
using System.IO;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberInstallDirValidator : IInstallDirValidator
    {
        public bool ValidateInstallDir([NotNullWhen(true)] string? path) =>
            !string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, "Beat Saber.exe"));
    }
}