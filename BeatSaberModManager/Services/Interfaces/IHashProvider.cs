using System.IO;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IHashProvider
    {
        string CalculateHashForFile(string path);
        string CalculateHashForStream(Stream stream);
    }
}