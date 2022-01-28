using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IGameVersionProvider
    {
        Task<string?> DetectGameVersionAsync(string installDir);
    }
}