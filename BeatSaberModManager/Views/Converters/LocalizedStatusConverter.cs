using System;
using System.Globalization;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

using BeatSaberModManager.Services.Implementations.Progress;


namespace BeatSaberModManager.Views.Converters
{
    public class LocalizedStatusConverter : IValueConverter
    {
        private readonly IResourceHost _resourceHost;

        public LocalizedStatusConverter() : this(Application.Current!) { }

        public LocalizedStatusConverter(IResourceHost resourceHost)
        {
            _resourceHost = resourceHost;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is ProgressInfo progressInfo ? Convert(progressInfo) : null;

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

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}