using System.Diagnostics.CodeAnalysis;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides a method to validate a game's installation.
    /// </summary>
    public interface IInstallDirValidator
    {
        /// <summary>
        /// Validates the game's installation.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <returns>True if the installation is valid, false otherwise.</returns>
        bool ValidateInstallDir([NotNullWhen(true)] string? path);
    }
}