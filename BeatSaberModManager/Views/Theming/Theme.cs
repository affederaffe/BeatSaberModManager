using Avalonia.Styling;


namespace BeatSaberModManager.Views.Theming
{
    public class Theme
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