using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Implementations.Http;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.ModelSaber
{
    public class ModelSaberModelInstaller
    {
        private readonly AppSettings _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly HttpProgressClient _httpClient;

        private const string kModelSaberFilesEndpoint = "https://modelsaber.com/files/";

        public ModelSaberModelInstaller(ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator, HttpProgressClient httpClient)
        {
            _appSettings = appSettings.Value;
            _installDirValidator = installDirValidator;
            _httpClient = httpClient;
        }

        public async Task<bool> InstallModelAsync(Uri uri, IStatusProgress? progress = null)
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.InstallDir.Value)) return false;
            string? folderName = uri.Host switch
            {
                "avatar" => "CustomAvatars",
                "saber" => "CustomSabers",
                "platform" => "CustomPlatforms",
                "bloq" => "CustomNotes",
                _ => null
            };

            if (folderName is null) return false;
            string folderPath = Path.Combine(_appSettings.InstallDir.Value!, folderName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string modelName = WebUtility.UrlDecode(uri.Segments.Last());
            progress?.Report(modelName);
            using HttpResponseMessage response = await _httpClient.GetAsync(kModelSaberFilesEndpoint + uri.Host + uri.AbsolutePath, progress);
            if (!response.IsSuccessStatusCode) return false;
            byte[] body = await response.Content.ReadAsByteArrayAsync();
            string filePath = Path.Combine(folderPath, modelName);
            await File.WriteAllBytesAsync(filePath, body);
            return true;
        }
    }
}