using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Views.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Localisation
{
    public class LocalisationManager : ReactiveObject, ILocalisationManager
    {
        private readonly ISettings<AppSettings> _appSettings;

        public LocalisationManager(ISettings<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            Languages = _supportedLanguageCodes.Select(LoadLanguage).ToArray();
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == _appSettings.Value.LanguageCode) ??
                               Languages.FirstOrDefault(x => x.CultureInfo.Name == CultureInfo.CurrentCulture.Name) ??
                               Languages[0];
        }

        public IReadOnlyList<ILanguage> Languages { get; }

        private ILanguage _selectedLanguage = null!;
        public ILanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
        }

        public void Initialize(Action<ILanguage> applyLanguage)
        {
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(x => x.SelectedLanguage).OfType<Language>();
            selectedLanguageObservable.Subscribe(applyLanguage);
            selectedLanguageObservable.Subscribe(l => _appSettings.Value.LanguageCode = l.CultureInfo.Name);
        }

        private static ILanguage LoadLanguage(string languageCode)
        {
            ResourceInclude resourceInclude = new() { Source = new Uri($"avares://{nameof(BeatSaberModManager)}/Resources/Localisation/{languageCode}.axaml") };
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            return new Language(cultureInfo, resourceInclude);
        }

        private static readonly string[] _supportedLanguageCodes = { "en", "de" };
    }
}