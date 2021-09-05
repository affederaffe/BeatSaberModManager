using System.Collections.ObjectModel;


namespace BeatSaberModManager.Views.Interfaces
{
    public interface IThemeManager
    {
        ITheme SelectedTheme { get; set; }
        ObservableCollection<ITheme> Themes { get; }
        void Initialize();
        void ReloadExternalThemes();
    }
}