using System;
using System.IO;
using System.Text.Json;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.Settings
{
    public sealed class AppSettingsProvider : ISettings<AppSettings>, IDisposable
    {
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
        private readonly string _saveDirPath;
        private readonly string _saveFilePath;

        public AppSettingsProvider(IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Combine(appDataFolderPath, nameof(BeatSaberModManager));
            _saveFilePath = Path.Combine(_saveDirPath, "settings.json");
        }

        private AppSettings? _value;
        public AppSettings Value => _value ??= Load();

        public void Dispose() => Save();

        private void Save()
        {
            string json = JsonSerializer.Serialize(Value);
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            File.WriteAllText(_saveFilePath, json);
        }

        private AppSettings Load()
        {
            AppSettings? appSettings = null;
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            if (File.Exists(_saveFilePath) && (appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_saveFilePath))) is not null && _installDirValidator.ValidateInstallDir(appSettings.InstallDir))
                return appSettings;
            appSettings ??= new AppSettings();
            appSettings.InstallDir = _installDirLocator.DetectInstallDir();
            return appSettings;
        }
    }
}