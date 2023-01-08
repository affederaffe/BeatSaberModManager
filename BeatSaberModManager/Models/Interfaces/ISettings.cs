using System.Threading.Tasks;


namespace BeatSaberModManager.Models.Interfaces
{
    /// <summary>
    /// Provides a generic getter for settings.
    /// </summary>
    /// <typeparam name="T">The type of the settings class.</typeparam>
    public interface ISettings<out T>
    {
        /// <summary>
        /// Gets the loaded settings instance.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Asynchronously loads the config.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Save the config to disk.
        /// </summary>
        void Save();
    }
}
