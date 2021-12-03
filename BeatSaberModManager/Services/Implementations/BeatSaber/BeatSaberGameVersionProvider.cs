using System.IO;
using System.Text;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber
{
    public class BeatSaberGameVersionProvider : IGameVersionProvider
    {
        private readonly ISettings<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;

        public BeatSaberGameVersionProvider(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator)
        {
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
        }

        public string? DetectGameVersion()
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value)) return null;
            string filePath = Path.Combine(_appSettings.Value.InstallDir.Value!, "Beat Saber_Data", "globalgamemanagers");
            using FileStream stream = File.OpenRead(filePath);
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