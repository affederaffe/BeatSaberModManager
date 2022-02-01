using System.Diagnostics.CodeAnalysis;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IInstallDirValidator
    {
        bool ValidateInstallDir([NotNullWhen(true)] string? path);
    }
}