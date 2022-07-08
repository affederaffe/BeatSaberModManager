namespace BeatSaberModManager.Models.Interfaces
{
    /// <summary>
    /// Provides a generic getter for settings.
    /// </summary>
    /// <typeparam name="T">The type of the settings class.</typeparam>
    public interface ISettings<out T>
    {
        /// <summary>
        /// Gets or loads the settings instance.
        /// </summary>
        T Value { get; }
    }
}
