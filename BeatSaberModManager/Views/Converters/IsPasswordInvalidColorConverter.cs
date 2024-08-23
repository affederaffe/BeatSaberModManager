using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;


namespace BeatSaberModManager.Views.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public class IsPasswordInvalidColorConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool isPasswordInvalid)
                throw new InvalidCastException();
            return isPasswordInvalid ? Brushes.Red : Brushes.Transparent;
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new InvalidOperationException();
    }
}
