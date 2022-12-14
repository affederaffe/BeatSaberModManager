using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    /// <inheritdoc />
    public partial class BeatSaberGameVersionProvider : IGameVersionProvider
    {
        /// <inheritdoc />
        public async Task<string?> DetectGameVersionAsync(string installDir)
        {
            string filePath = Path.Join(installDir, "Beat Saber_Data", "globalgamemanagers");
            await using FileStream? fileStream = IOUtils.TryOpenFile(filePath, new FileStreamOptions { Options = FileOptions.Asynchronous });
            if (fileStream is null) return null;
            using BinaryReader reader = new(fileStream, Encoding.UTF8);
            const string key = "public.app-category.games";

            SearchForKey(reader, key);
            if (fileStream.Position == fileStream.Length) return null; // we went through the entire stream without finding the key
            SearchForDigit(reader);

            const int rewind = -sizeof(int) - sizeof(byte);
            fileStream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

            string str = reader.ReadString();
            Regex regex = VersionRegex();
            Match match = regex.Match(str);
            return !match.Success ? null : match.Value;
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

        private static void SearchForDigit(BinaryReader reader)
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                char current = (char)reader.ReadByte();
                if (char.IsDigit(current)) break;
            }
        }

        [GeneratedRegex("[\\d]+.[\\d]+.[\\d]+")]
        private static partial Regex VersionRegex();
    }
}
