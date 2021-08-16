namespace BeatSaberModManager.Models.Implementations.Interfaces
{
    public interface IInstallDirLocator
    {
        bool TryDetectInstallDir(out string? installDir);
    }
}