using System.IO;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IHashProvider
    {
        Task<string?> CalculateHashForFile(string path);
        Task<string?> CalculateHashForStream(Stream stream);
    }
}