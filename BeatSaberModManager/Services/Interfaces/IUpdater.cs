using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IUpdater
    {
        Task<bool> NeedsUpdate();
        Task<int> Update();
    }
}