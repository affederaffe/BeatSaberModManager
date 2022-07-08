using Avalonia.Styling;


namespace BeatSaberModManager.Views.Theming
{
    /// <summary>
    /// Represents a theme for Avalonia.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Theme"/> class.
        /// </summary>
        public Theme(string name, IStyle style)
        {
            Name = name;
            Style = style;
        }

        /// <summary>
        /// The name of the theme.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The associated <see cref="IStyle"/> of the theme.
        /// </summary>
        public IStyle Style { get; }
    }
}
