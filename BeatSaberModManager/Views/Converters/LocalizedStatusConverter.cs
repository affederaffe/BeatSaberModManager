using System;
using System.Globalization;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

using BeatSaberModManager.Models.Implementations.Progress;


namespace BeatSaberModManager.Views.Converters
{
    /// <summary>
    /// Converts the <see cref="StatusType"/> of a <see cref="ProgressInfo"/> to a localized string.
    /// </summary>
    public class LocalizedStatusConverter : IValueConverter
    {
        private readonly IResourceHost _resourceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedStatusConverter"/> class with <see cref="Application.Current"/> as the <see cref="IResourceHost"/>
        /// </summary>
        public LocalizedStatusConverter() : this(Application.Current!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedStatusConverter"/> class with a specified <see cref="IResourceHost"/>
        /// </summary>
        public LocalizedStatusConverter(IResourceHost resourceHost)
        {
            _resourceHost = resourceHost;
        }

        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is ProgressInfo progressInfo ? Convert(progressInfo) : null;

        /// <summary>
        /// Converts the <see cref="StatusType"/> of a <see cref="ProgressInfo"/> to a localized string
        /// </summary>
        /// <param name="progressInfo">The <see cref="ProgressInfo"/> to convert</param>
        /// <returns>The localized string</returns>
        public string Convert(ProgressInfo progressInfo)
        {
            string? localizedStatus = progressInfo.StatusType switch
            {
                StatusType.Installing => _resourceHost.FindResource($"Status:{nameof(StatusType.Installing)}") as string,
                StatusType.Uninstalling => _resourceHost.FindResource($"Status:{nameof(StatusType.Uninstalling)}") as string,
                StatusType.Completed => _resourceHost.FindResource($"Status:{nameof(StatusType.Completed)}") as string,
                StatusType.Failed => _resourceHost.FindResource($"Status:{nameof(StatusType.Failed)}") as string,
                _ => string.Empty
            };

            return $"{localizedStatus} {progressInfo.Text}";
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
