using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data.Converters;


namespace BeatSaberModManager.Views.Converters
{
    /// <summary>
    /// Converts a <see cref="Thickness"/> to only use its <see cref="Thickness.Top"/> value.
    /// </summary>
    public class TopOnlyThicknessConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value switch
            {
                Thickness thickness => new Thickness(0, thickness.Top, 0, 0),
                double d => new Thickness(0, d, 0, 0),
                _ => new Thickness()
            };

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new InvalidOperationException();
    }
}
