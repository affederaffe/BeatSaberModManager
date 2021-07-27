using System;
using System.IO;
using System.Text.Json;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;


namespace BeatSaberModManager.Models
{
    public class Settings
    {
        public string? InstallDir { get; set; }
        public string? VRPlatform { get; set; }
        public string? ThemesDir { get; set; }
        public string? ThemeName { get; set; }
        public string? LanguageName { get; set; }

        public Settings()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Exit += (_, _) => Save();
        }

        private void Save()
        {
            JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, jsonSerializerOptions);
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            File.WriteAllText(_saveFilePath, json);
        }

        private static readonly string _saveDirPath;
        private static readonly string _saveFilePath;

        static Settings()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Combine(appDataFolderPath, "BeatSaberModManager");
            _saveFilePath = Path.Combine(_saveDirPath, "settings.json");
        }

        public static Settings Load()
        {
            Settings? settings = null;
            if (!Directory.Exists(_saveDirPath)) Directory.CreateDirectory(_saveDirPath);
            if (File.Exists(_saveFilePath)) settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(_saveFilePath));
            return settings ?? new Settings();
        }
    }
}