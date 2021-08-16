using System.IO;


namespace BeatSaberModManager.Models.Implementations.Interfaces
{
    public interface IHashProvider
    {
        string CalculateHashForFile(string path);
        string CalculateHashForStream(Stream stream);
    }
}