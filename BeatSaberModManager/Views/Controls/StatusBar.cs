using Avalonia;
using Avalonia.Controls;

using BeatSaberModManager.Models.Implementations.Progress;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// TODO
    /// </summary>
    public class StatusBar : ContentControl
    {
        /// <summary>
        /// Defines the <see cref="Text"/> property.
        /// </summary>
        public static readonly StyledProperty<string?> TextProperty = TextBlock.TextProperty.AddOwner<StatusBar>();

        /// <summary>
        /// Defines the <see cref="StatusType"/> property.
        /// </summary>
        public static readonly StyledProperty<StatusType> StatusTypeProperty = AvaloniaProperty.Register<StatusBar, StatusType>(nameof(ProgressValue));

        /// <summary>
        /// Defines the <see cref="ProgressValue"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ProgressValueProperty = AvaloniaProperty.Register<StatusBar, double>(nameof(ProgressValue));

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        public StatusType? StatusType
        {
            get => GetValue(StatusTypeProperty);
            set => SetValue(StatusTypeProperty, value);
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double ProgressValue
        {
            get => GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }
    }
}
