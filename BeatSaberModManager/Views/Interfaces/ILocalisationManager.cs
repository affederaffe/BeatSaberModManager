namespace BeatSaberModManager.Views.Interfaces
{
    public interface ILocalisationManager
    {
        ILanguage SelectedLanguage { get; set; }
        ILanguage[] Languages { get; }
        void Initialize();
    }
}