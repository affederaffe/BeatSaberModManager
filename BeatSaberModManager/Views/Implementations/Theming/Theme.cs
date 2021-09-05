using Avalonia.Styling;

using BeatSaberModManager.Views.Interfaces;


namespace BeatSaberModManager.Views.Implementations.Theming
{
    public class Theme : ITheme
    {
        public Theme(string name, IStyle style)
        {
            Name = name;
            Style = style;
        }

        public string Name { get; }
        public IStyle Style { get; }
    }
}