using System;
using System.IO;
using System.Text.Json;

using BeatSaberModManager.Models.Interfaces;

using Microsoft.Extensions.Options;


namespace BeatSaberModManager.Models.Implementations.Settings
{
    public sealed class SettingsManager : IOptions<SettingsStore>, IDisposable
    {
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;
        private readonly string _saveDirPath;
        private readonly string _saveFilePath;

        public SettingsManager(IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Combine(appDataFolderPath, nameof(BeatSaberModManager));
            _saveFilePath = Path.Combine(_saveDirPath, "settings.json");
            Value = Load();
        }

        public SettingsStore Value { get; }

        public void Dispose() => Save();

        private void Save()
        {
            string json = JsonSerializer.Serialize(Value);
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            File.WriteAllText(_saveFilePath, json);
        }

        private SettingsStore Load()
        {
            SettingsStore? settingsStore = null;
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            if (File.Exists(_saveFilePath) && (settingsStore = JsonSerializer.Deserialize<SettingsStore>(File.ReadAllText(_saveFilePath))) is not null
                                           && _installDirValidator.ValidateInstallDir(settingsStore.InstallDir))
                return settingsStore;
            settingsStore ??= new SettingsStore();
            settingsStore.InstallDir = _installDirLocator.DetectInstallDir();
            return settingsStore;
        }
    }
}