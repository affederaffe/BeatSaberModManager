using Avalonia.Styling;

using ReactiveUI;


namespace BeatSaberModManager.ThemeManagement
{
    public class Theme : ReactiveObject
    {
        internal Theme(string name, IStyle style)
        {
            _name = name;
            _style = style;
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private IStyle _style;
        public IStyle Style
        {
            get => _style;
            set => this.RaiseAndSetIfChanged(ref _style, value);
        }
    }
}