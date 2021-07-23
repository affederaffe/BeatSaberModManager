using System.IO;
using System.Text;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber
{
    public class BeatSaberGameVersionProvider : IGameVersionProvider
    {
        private readonly Settings _settings;

        public BeatSaberGameVersionProvider(Settings settings)
        {
            _settings = settings;
        }

        public string? GetGameVersion()
        {
            string filename = Path.Combine(_settings.InstallDir!, "Beat Saber_Data", "globalgamemanagers");
            using FileStream stream = File.OpenRead(filename);
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

            while (stream.Position < stream.Length)
            {
                char current = (char)reader.ReadByte();
                if (char.IsDigit(current))
                    break;
            }

            const int rewind = -sizeof(int) - sizeof(byte);
            stream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

            int strlen = reader.ReadInt32();
            byte[] strbytes = reader.ReadBytes(strlen);
            return Encoding.UTF8.GetString(strbytes);
        }
    }
}