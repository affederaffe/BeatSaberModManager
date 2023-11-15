using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using Serilog;


namespace BeatSaberModManager.Services.Implementations.Settings
{
    /// <summary>
    /// Automatically loads and saves <typeparamref name="T"/> as a json file.
    /// </summary>
    /// <typeparam name="T">The type of the settings class.</typeparam>
    public sealed class JsonSettingsProvider<T> : ISettings<T>, IAsyncDisposable where T : class, new()
    {
        private readonly ILogger _logger;
        private readonly JsonTypeInfo<T> _jsonTypeInfo;
        private readonly string _saveDirPath;
        private readonly string _saveFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSettingsProvider{T}"/> class.
        /// </summary>
        public JsonSettingsProvider(ILogger logger, JsonTypeInfo<T> jsonTypeInfo)
        {
            _logger = logger;
            _jsonTypeInfo = jsonTypeInfo;
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirPath = Path.Join(appDataFolderPath, ThisAssembly.Info.Product);
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

#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(_saveFilePath, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2007
            if (fileStream is null)
            {
                Value = new T();
                return;
            }

            try
            {
                Value = await JsonSerializer.DeserializeAsync(fileStream, _jsonTypeInfo).ConfigureAwait(false) ?? new T();
            }
            catch (JsonException e)
            {
                _logger.Warning(e, "Invalid config, deleting");
                Value = new T();
                IOUtils.TryDeleteFile(_saveFilePath);
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync()
        {
            if (!IOUtils.TryCreateDirectory(_saveDirPath))
                return;
#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(_saveFilePath, FileMode.Create, FileAccess.Write);
#pragma warning restore CA2007
            if (fileStream is not null)
                await JsonSerializer.SerializeAsync(fileStream, Value, _jsonTypeInfo).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() => await SaveAsync().ConfigureAwait(false);
    }
}
