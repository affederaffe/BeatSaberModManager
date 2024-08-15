using System.Diagnostics.CodeAnalysis;
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

            if (!TrySeekToKey(reader, key))
                return null;

            if (!TryFindVersion(reader, out string? version))
                return null;

            Regex regex = VersionRegex();
            Match match = regex.Match(version);
            return !match.Success ? null : match.Value;
        }

        private static bool TrySeekToKey(BinaryReader reader, string key)
        {
            int pos = 0;
            Stream stream = reader.BaseStream;
            while (stream.Position < stream.Length && pos < key.Length)
            {
                if (reader.ReadByte() == key[pos])
                    pos++;
                else
                    pos = 0;
            }

            return stream.Position == stream.Length; // we went through the entire stream without finding the key
        }

        private static bool TryFindVersion(BinaryReader reader, [NotNullWhen(true)] out string? version)
        {
            Stream stream = reader.BaseStream;
            long streamLength = stream.Length;
            while (stream.Position < streamLength)
            {
                char current = (char)reader.ReadByte();
                if (!char.IsDigit(current))
                    continue;

                long startPos = stream.Position - 1;
                int dotCount = 0;

                // for each first occurrence of a digit, try if it is a version of the form Major.Minor.Patch, i.e. with 2 dots
                while (stream.Position < streamLength)
                {
                    current = (char)reader.ReadByte();
                    if (char.IsDigit(current))
                    {
                        if (dotCount != 2 || stream.Position >= streamLength || char.IsDigit((char)reader.PeekChar()))
                            continue;

                        // found a version of the form Major.Minor.Patch
                        int length = (int)(stream.Position - startPos);
                        stream.Seek(-length, SeekOrigin.Current); // rewind to the string length
                        byte[] bytes = reader.ReadBytes(length);
                        version = Encoding.UTF8.GetString(bytes);
                        return true;
                    }

                    if (current == '.')
                        dotCount++;
                    else
                        break; // digit occurence is only a numeral literal, no version
                }
            }

            version = null;
            return false;
        }

        // There is one version ending in "p1" on BeatMods
        [GeneratedRegex(@"[\d]+.[\d]+.[\d]+(p1)?")]
        private static partial Regex VersionRegex();
    }
}
