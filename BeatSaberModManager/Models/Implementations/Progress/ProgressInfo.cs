using System;


namespace BeatSaberModManager.Models.Implementations.Progress
{
    /// <summary>
    /// Represents information about the current operation.
    /// </summary>
    public readonly struct ProgressInfo(StatusType statusType, string? text) : IEquatable<ProgressInfo>
    {
        /// <summary>
        /// The status of the current operation.
        /// </summary>
        public StatusType StatusType { get; } = statusType;

        /// <summary>
        /// The message to display.
        /// </summary>
        public string? Text { get; } = text;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ProgressInfo progressInfo && this == progressInfo;

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine((int)StatusType, Text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ProgressInfo left, ProgressInfo right) => left.Equals(right);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ProgressInfo left, ProgressInfo right) => !(left == right);

        /// <inheritdoc />
        public bool Equals(ProgressInfo other) => StatusType == other.StatusType && Text == other.Text;
    }
}
