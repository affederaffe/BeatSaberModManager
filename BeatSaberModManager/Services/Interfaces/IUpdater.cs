using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Defines a method for updating the application.
    /// </summary>
    public interface IUpdater
    {
        /// <summary>
        /// Asynchronously checks for an update of the application.
        /// </summary>
        /// <returns>True when a newer version is available, false otherwise.</returns>
        Task<bool> NeedsUpdate();

        /// <summary>
        /// Asynchronously updates the application.
        /// </summary>
        /// <returns>0 if the update succeeds, -1 otherwise.</returns>
        Task<int> Update();
    }
}