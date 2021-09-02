using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;

using Microsoft.Extensions.Options;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberModelInstaller
    {
        private readonly SettingsStore _settingsStore;
        private readonly HttpClient _httpClient;

        private const string kModelSaberFilesEndpoint = "https://modelsaber.com/files/";

        public ModelSaberModelInstaller(IOptions<SettingsStore> settingsStore, HttpClient httpClient)
        {
            _settingsStore = settingsStore.Value;
            _httpClient = httpClient;
        }

        public async Task<bool> InstallModelAsync(Uri uri, IStatusProgress? progress = null)
        {
            if (!Directory.Exists(_settingsStore.InstallDir)) return false;
            string? folderName = uri.Host switch
            {
                "avatar" => "CustomAvatars",
                "saber" => "CustomSabers",
                "platform" => "CustomPlatforms",
                "bloq" => "CustomNotes",
                _ => null
            };

            if (folderName is null) return false;
            string folderPath = Path.Combine(_settingsStore.InstallDir, folderName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string modelName = WebUtility.UrlDecode(uri.Segments.Last());
            progress?.Report(modelName);
            using HttpResponseMessage response = await _httpClient.GetAsync(kModelSaberFilesEndpoint + uri.Host + uri.AbsolutePath);
            if (!response.IsSuccessStatusCode) return false;
            byte[] body = await response.Content.ReadAsByteArrayAsync();
            string filePath = Path.Combine(folderPath, modelName);
            await File.WriteAllBytesAsync(filePath, body);
            return true;
        }
    }
}