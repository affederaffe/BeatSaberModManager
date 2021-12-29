using System;
using System.Collections.Generic;


namespace BeatSaberModManager.Views.Interfaces
{
    public interface ILocalizationManager
    {
        ILanguage SelectedLanguage { get; set; }
        IReadOnlyList<ILanguage> Languages { get; }
        void Initialize(Action<ILanguage> applyLanguage);
    }
}