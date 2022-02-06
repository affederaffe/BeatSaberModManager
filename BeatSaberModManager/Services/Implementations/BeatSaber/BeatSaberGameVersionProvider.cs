using System.IO;
using System.Text;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public class BeatSaberGameVersionProvider : IGameVersionProvider
    {
        private string? _gameVersion;

        /// <inheritdoc />
        public async Task<string?> DetectGameVersionAsync(string installDir) =>
            _gameVersion ?? await DetectGameVersionAsyncCore(installDir);

        private async Task<string?> DetectGameVersionAsyncCore(string installDir)
        {
            string filePath = Path.Combine(installDir, "Beat Saber_Data", "globalgamemanagers");
            await using FileStream? fileStream = IOUtils.TryOpenFile(filePath, new FileStreamOptions { Options = FileOptions.Asynchronous });
            if (fileStream is null) return null;
            using BinaryReader reader = new(fileStream, Encoding.UTF8);
            const string key = "public.app-category.games";

            SearchForKey(reader, key);
            if (fileStream.Position == fileStream.Length) return null; // we went through the entire stream without finding the key
            ReadWhileDigit(reader);

            const int rewind = -sizeof(int) - sizeof(byte);
            fileStream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);
            _gameVersion = Encoding.UTF8.GetString(bytes);
            return _gameVersion;
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
                if (char.IsDigit(current)) break;
            }
        }
    }
}