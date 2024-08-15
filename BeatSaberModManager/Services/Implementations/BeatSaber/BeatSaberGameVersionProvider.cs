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
#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(filePath, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2007
            if (fileStream is null)
                return null;
            using BinaryReader reader = new(fileStream, Encoding.UTF8);
            const string key = "public.app-category.games";

            SearchForKey(reader, key);
            if (fileStream.Position == fileStream.Length)
                return null; // we went through the entire stream without finding the key
            (long startIndex, long endIndex) = SearchForVersion(reader);

            int length = (int)(endIndex - startIndex);
            _ = fileStream.Seek(-length, SeekOrigin.Current); // rewind to the string length
            byte[] bytes = reader.ReadBytes(length);
            string str = Encoding.UTF8.GetString(bytes);
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

        private static (long, long) SearchForVersion(BinaryReader reader)
        {
            long startIndex = 0;
            long endIndex = 0;
            Stream stream = reader.BaseStream;
            long streamLength = stream.Length;
            while (stream.Position < streamLength && endIndex == 0)
            {
                char current = (char)reader.ReadByte();
                if (char.IsDigit(current))
                {
                    startIndex = stream.Position - 1;
                    int dotCount = 0;

                    while (stream.Position < streamLength)
                    {
                        current = (char)reader.ReadByte();
                        if (char.IsDigit(current))
                        {
                            if (dotCount == 2 && stream.Position < streamLength && !char.IsDigit((char)reader.PeekChar()))
                            {
                                endIndex = stream.Position;
                                break;
                            }
                        }
                        else if (current == '.')
                        {
                            dotCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return (startIndex, endIndex);
        }

        [GeneratedRegex(@"[\d]+.[\d]+.[\d]+")]
        private static partial Regex VersionRegex();
    }
}
