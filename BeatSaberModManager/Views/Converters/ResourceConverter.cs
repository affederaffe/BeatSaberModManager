using System;
using System.Globalization;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;


namespace BeatSaberModManager.Views.Converters
{
    /// <summary>
    /// Converts a key to the corresponding resource.
    /// </summary>
    public class ResourceConverter : IValueConverter
    {
        private readonly IResourceHost _resourceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedStatusConverter"/> class with <see cref="Application.Current"/> as the <see cref="IResourceHost"/>
        /// </summary>
        public ResourceConverter() : this(Application.Current!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedStatusConverter"/> class with a specified <see cref="IResourceHost"/>
        /// </summary>
        public ResourceConverter(IResourceHost resourceHost)
        {
            _resourceHost = resourceHost;
        }

        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is null ? null : _resourceHost.FindResource(value);

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
