using System;
using System.IO;
using System.Text.Json;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utilities;


namespace BeatSaberModManager.Services.Implementations.Settings
{
    public sealed class JsonSettingsProvider<T> : ISettings<T>, IDisposable where T : new()
    {
        private readonly string _saveDirPath;
        private readonly string _saveFilePath;

        public JsonSettingsProvider()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Combine(appDataFolderPath, nameof(BeatSaberModManager));
            _saveFilePath = Path.Combine(_saveDirPath, $"{typeof(T).Name}.json");
        }

        private T? _value;
        public T Value => _value ??= Load();

        public void Dispose() => Save();

        private void Save()
        {
            string json = JsonSerializer.Serialize(Value, new JsonSerializerOptions { WriteIndented = true });
            IOUtils.TryCreateDirectory(_saveDirPath);
            File.WriteAllText(_saveFilePath, json);
        }

        private T Load()
        {
            IOUtils.TryCreateDirectory(_saveDirPath);
            if (!IOUtils.TryReadAllText(_saveFilePath, out string? json)) return new T();
            T? settings = JsonSerializer.Deserialize<T>(json);
            settings ??= new T();
            return settings;
        }
    }
}