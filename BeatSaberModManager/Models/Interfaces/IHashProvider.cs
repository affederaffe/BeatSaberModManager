using System.IO;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IHashProvider
    {
        string CalculateHashForFile(string path);
        string CalculateHashForStream(Stream stream);
    }
}