using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IInstallDirLocator
    {
        ValueTask<string?> LocateInstallDirAsync();
    }
}