using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;


namespace BeatSaberModManager.Views.Converters
{
    /// <summary>
    /// Converts a boolean value to an <see cref="IBrush"/>.
    /// </summary>
    public class IsUpToDateColorConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b ?
                b ? Brushes.Green : Brushes.Red
                : null;

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}