using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberModelInstaller
    {
        private readonly Settings _settings;
        private readonly HttpClient _httpClient;

        private const string kModelSaberFilesEndpoint = "https://modelsaber.com/files/";

        public ModelSaberModelInstaller(Settings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public async Task<bool> InstallModelAsync(Uri uri, IStatusProgress? progress = null)
        {
            if (!Directory.Exists(_settings.InstallDir)) return false;
            string? folderName = uri.Host switch
            {
                "avatar" => "CustomAvatars",
                "saber" => "CustomSabers",
                "platform" => "CustomPlatforms",
                "bloq" => "CustomNotes",
                _ => null
            };

            if (folderName is null) return false;
            string folderPath = Path.Combine(_settings.InstallDir, folderName);
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