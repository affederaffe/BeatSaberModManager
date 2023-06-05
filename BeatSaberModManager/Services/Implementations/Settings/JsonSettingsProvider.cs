using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.Settings
{
    /// <summary>
    /// Automatically loads and saves <typeparamref name="T"/> as a json file.
    /// </summary>
    /// <typeparam name="T">The type of the settings class.</typeparam>
    public sealed class JsonSettingsProvider<T> : ISettings<T>, IAsyncDisposable where T : class, new()
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
            _saveDirPath = Path.Join(appDataFolderPath, Program.Product);
            _saveFilePath = Path.Join(_saveDirPath, $"{typeof(T).Name}.json");
        }

        /// <summary>
        /// The instance of the loaded setting <typeparamref name="T"/>.
        /// </summary>
        public T Value { get; private set; } = null!;

        /// <inheritdoc />
        public async Task LoadAsync()
        {
            if (!IOUtils.TryCreateDirectory(_saveDirPath))
            {
                Value = new T();
                return;
            }

            await using FileStream? fileStream = IOUtils.TryOpenFile(_saveFilePath, new FileStreamOptions { Options = FileOptions.Asynchronous });
            if (fileStream is null)
            {
                Value = new T();
                return;
            }

            try
            {
                Value = await JsonSerializer.DeserializeAsync(fileStream, _jsonTypeInfo) ?? new T();
            }
            catch
            {
                Value = new T();
                IOUtils.TryDeleteFile(_saveFilePath);
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync()
        {
            if (!IOUtils.TryCreateDirectory(_saveDirPath))
                return;
            await using FileStream? fileStream = IOUtils.TryOpenFile(_saveFilePath, new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Create });
            if (fileStream is not null)
                await JsonSerializer.SerializeAsync(fileStream, Value, _jsonTypeInfo);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() => await SaveAsync();
    }
}
