using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using BeatSaberModManager.Utils;

using Microsoft.Extensions.Options;


namespace BeatSaberModManager.Services.Implementations.Settings
{
    /// <summary>
    /// Automatically loads and saves <typeparamref name="T"/> as a json file.
    /// </summary>
    /// <typeparam name="T">The type of the settings class.</typeparam>
    public sealed class JsonSettingsProvider<T> : IOptions<T>, IDisposable where T : class, new()
    {
        private readonly JsonTypeInfo<T> _jsonTypeInfo;
        private readonly string _saveDirPath;
        private readonly string _saveFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSettingsProvider{T}"/> class.
        /// </summary>
        public JsonSettingsProvider(JsonTypeInfo<T> jsonTypeInfo)
        {
            _jsonTypeInfo = jsonTypeInfo;
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Combine(appDataFolderPath, nameof(BeatSaberModManager));
            _saveFilePath = Path.Combine(_saveDirPath, $"{typeof(T).Name}.json");
        }

        /// <summary>
        /// The instance of the loaded setting <typeparamref name="T"/>.
        /// </summary>
        public T Value => _value ??= Load();
        private T? _value;

        /// <inheritdoc />
        public void Dispose() => Save();

        private void Save()
        {
            string json = JsonSerializer.Serialize(Value, _jsonTypeInfo);
            if (IOUtils.TryCreateDirectory(_saveDirPath)) File.WriteAllText(_saveFilePath, json);
        }

        private T Load()
        {
            if (!IOUtils.TryCreateDirectory(_saveDirPath) || !IOUtils.TryReadAllText(_saveFilePath, out string? json)) return new T();
            T? settings = JsonSerializer.Deserialize(json, _jsonTypeInfo);
            settings ??= new T();
            return settings;
        }
    }
}