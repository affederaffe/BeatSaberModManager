using System.Diagnostics.CodeAnalysis;
using System.IO;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberInstallDirValidator : IInstallDirValidator
    {
        /// <inheritdoc />
        public bool ValidateInstallDir([NotNullWhen(true)] string? path) =>
            !string.IsNullOrEmpty(path) && File.Exists(Path.Join(path, "Beat Saber.exe"));
    }
}