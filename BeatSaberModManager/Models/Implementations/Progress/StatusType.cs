namespace BeatSaberModManager.Models.Implementations.Progress
{
    /// <summary>
    /// Represents the status of an operation
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// There is no running operation
        /// </summary>
        None,

        /// <summary>
        /// The operation is currently installing a resource
        /// </summary>
        Installing,

        /// <summary>
        /// The operation is currently uninstalling a resource
        /// </summary>
        Uninstalling,

        /// <summary>
        /// The operation ran to completion successfully
        /// </summary>
        Completed,

        /// <summary>
        /// The operation failed
        /// </summary>
        Failed
    }
}