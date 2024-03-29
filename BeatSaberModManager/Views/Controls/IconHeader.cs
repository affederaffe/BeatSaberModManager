using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// A Header with a dockable icon.
    /// </summary>
    public class IconHeader : TemplatedControl
    {
        /// <inheritdoc cref="ContentControl.ContentProperty"/>
        public static readonly StyledProperty<object?> ContentProperty = ContentControl.ContentProperty.AddOwner<IconHeader>();

        /// <summary>
        /// Defines the IconDataProperty.
        /// </summary>
        public static readonly StyledProperty<Geometry> IconDataProperty = AvaloniaProperty.Register<IconHeader, Geometry>(nameof(IconData));

        /// <summary>
        /// Defines the TextPositionProperty.
        /// </summary>
        public static readonly StyledProperty<Dock> IconPlacementProperty = AvaloniaProperty.Register<IconHeader, Dock>(nameof(IconPlacement), Dock.Top);

        /// <inheritdoc cref="ContentControl.Content"/>
        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the icon data of the IconHeader.
        /// </summary>
        public Geometry IconData
        {
            get => GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }

        /// <summary>
        /// Gets or sets the content placement of the IconHeader.
        /// </summary>
        public Dock IconPlacement
        {
            get => GetValue(IconPlacementProperty);
            set => SetValue(IconPlacementProperty, value);
        }
    }
}
