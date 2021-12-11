using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IGameVersionProvider
    {
        Task<string?> DetectGameVersion(string installDir);
    }
}