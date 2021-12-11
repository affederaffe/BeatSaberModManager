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

            SearchForKey(reader, key);
            if (stream.Position == stream.Length) return null; // we went through the entire stream without finding the key
            ReadWhileDigit(reader);

            const int rewind = -sizeof(int) - sizeof(byte);
            stream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }

        private static void SearchForKey(BinaryReader reader, string key)
        {
            int pos = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length && pos < key.Length)
            {
                if (reader.ReadByte() == key[pos]) pos++;
                else pos = 0;
            }
        }

        private static void ReadWhileDigit(BinaryReader reader)
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                char current = (char)reader.ReadByte();
                if (char.IsDigit(current))
                    break;
            }
        }
    }
}