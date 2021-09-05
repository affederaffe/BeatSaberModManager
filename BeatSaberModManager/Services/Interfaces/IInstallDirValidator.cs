namespace BeatSaberModManager.Services.Interfaces
{
    public interface IInstallDirValidator
    {
        bool ValidateInstallDir(string? path);
        string DetectVRPlatform(string path);
    }
}