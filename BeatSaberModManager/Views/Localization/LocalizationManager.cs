using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.Views.Localization
{
    /// <summary>
    /// Load and apply localization settings.
    /// </summary>
    public class LocalizationManager : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationManager"/> class.
        /// </summary>
        public LocalizationManager(ISettings<AppSettings> appSettings, Application application)
        {
            Languages = _supportedLanguageCodes.Select(LoadLanguage).ToArray();
            _selectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == appSettings.Value.LanguageCode) ??
                                Languages.FirstOrDefault(static x => x.CultureInfo.Name == CultureInfo.CurrentCulture.Name) ??
                                Languages[0];
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(static x => x.SelectedLanguage);
            selectedLanguageObservable.Subscribe(l => application.Resources.MergedDictionaries[0] = l.ResourceProvider);
            selectedLanguageObservable.Subscribe(l => appSettings.Value.LanguageCode = l.CultureInfo.Name);
        }

        /// <summary>
        /// A collection of all available <see cref="Language"/>s.
        /// </summary>
        public IReadOnlyList<Language> Languages { get; }

        /// <summary>
        /// The currently selected <see cref="Language"/>.
        /// </summary>
        public Language SelectedLanguage
        {
            get => _selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
        }

        private Language _selectedLanguage;

        private static Language LoadLanguage(string languageCode)
        {
            ResourceInclude resourceInclude = new() { Source = new Uri($"avares://BeatSaberModManager/Resources/Localization/{languageCode}.axaml") };
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            return new Language(cultureInfo, resourceInclude);
        }

        private static readonly string[] _supportedLanguageCodes = { "en", "de" };
    }
}