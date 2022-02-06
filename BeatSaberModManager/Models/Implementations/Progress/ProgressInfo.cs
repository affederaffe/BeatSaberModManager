namespace BeatSaberModManager.Models.Implementations.Progress
{
    /// <summary>
    /// Represents information about the current operation
    /// </summary>
    public readonly struct ProgressInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressInfo"/> struct.
        /// </summary>
        public ProgressInfo(StatusType statusType, string? text)
        {
            StatusType = statusType;
            Text = text;
        }

        /// <summary>
        /// The status of the current operation
        /// </summary>
        public StatusType StatusType { get; }

        /// <summary>
        /// The message to display
        /// </summary>
        public string? Text { get; }
    }
}