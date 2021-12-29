using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;


namespace BeatSaberModManager.Views.Implementations.Converters
{
    public class IsUpToDateColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b ?
                b ? Brushes.Green : Brushes.Red
                : null;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}