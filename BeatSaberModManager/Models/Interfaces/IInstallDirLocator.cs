namespace BeatSaberModManager.Models.Interfaces
{
    public interface IInstallDirLocator
    {
        bool TryDetectInstallDir(out string? installDir);
    }
}