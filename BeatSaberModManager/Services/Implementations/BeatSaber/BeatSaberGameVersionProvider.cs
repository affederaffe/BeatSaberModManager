using System.IO;
using System.Text;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberGameVersionProvider : IGameVersionProvider
    {
        public async Task<string?> DetectGameVersion(string installDir)
        {
            string filePath = Path.Combine(installDir, "Beat Saber_Data", "globalgamemanagers");
            await using FileStream? stream = IOUtils.SafeOpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous);
            if (stream is null) return null;
            using BinaryReader reader = new(stream, Encoding.UTF8);
            const string key = "public.app-category.games";

            int pos = 0;
            while (stream.Position < stream.Length && pos < key.Length)
            {
                if (reader.ReadByte() == key[pos]) pos++;
                else pos = 0;
            }

            if (stream.Position == stream.Length) // we went through the entire stream without finding the key
                return null;

            while (stream.Position < stream.Length && !char.IsDigit((char)reader.ReadByte())) { }

            const int rewind = -sizeof(int) - sizeof(byte);
            stream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}