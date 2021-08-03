using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberAssetProvider : IAssetProvider
    {
        private readonly Settings _settings;
        private readonly HttpClient _httpClient;

        private const string kModelSaberUrlPrefix = "https://modelsaber.com/files/";

        public ModelSaberAssetProvider(Settings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public string Protocol => "modelsaber";

        public async Task<bool> InstallAssetAsync(Uri uri)
        {
            if (_settings.InstallDir is null) return false;
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
            HttpResponseMessage response = await _httpClient.GetAsync(kModelSaberUrlPrefix + uri.Host + uri.AbsolutePath);
            if (!response.IsSuccessStatusCode) return false;
            byte[] body = await response.Content.ReadAsByteArrayAsync();
            string filePath = WebUtility.UrlDecode(Path.Combine(folderPath, uri.Segments.Last()));
            await File.WriteAllBytesAsync(filePath, body);
            return true;
        }
    }
}