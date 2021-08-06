using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


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

        public async Task<bool> InstallModelFromUriAsync(Uri uri)
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
            HttpResponseMessage response = await _httpClient.GetAsync(kModelSaberFilesEndpoint + uri.Host + uri.AbsolutePath);
            if (!response.IsSuccessStatusCode) return false;
            byte[] body = await response.Content.ReadAsByteArrayAsync();
            string filePath = WebUtility.UrlDecode(Path.Combine(folderPath, uri.Segments.Last()));
            await File.WriteAllBytesAsync(filePath, body);
            return true;
        }
    }
}