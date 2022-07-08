namespace BeatSaberModManager.Models.Implementations
{
    /// <summary>
    /// Indicates from which store the game has been installed.
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// The game was installed through Steam.
        /// </summary>
        Steam,

        /// <summary>
        /// The game was installed through the Oculus store.
        /// </summary>
        Oculus
    }
}
