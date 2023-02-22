using System;
using System.Globalization;

using Avalonia.Data.Converters;

using BeatSaberModManager.Models.Implementations.Progress;


namespace BeatSaberModManager.Views.Converters
{
    /// <summary>
    /// Converts the <see cref="StatusType"/> of a <see cref="ProgressInfo"/> to a localized string.
    /// </summary>
    public class StatusTypeEnumConverter : IValueConverter
    {

        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is StatusType progressInfo ? Convert(progressInfo) : null;

        /// <summary>
        /// Converts the <see cref="StatusType"/> of a <see cref="ProgressInfo"/> to a localized string
        /// </summary>
        /// <param name="statusType">The <see cref="StatusType"/> to convert</param>
        /// <returns>The localized string</returns>
        public static string Convert(StatusType statusType) =>
            statusType switch
            {
                StatusType.Installing => "Status:Installing",
                StatusType.Uninstalling => "Status:Uninstalling",
                StatusType.Completed => "Status:Completed",
                StatusType.Failed => "Status:Failed",
                _ => string.Empty
            };

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
