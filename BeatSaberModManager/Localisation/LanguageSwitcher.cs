using System;
using System.Globalization;
using System.Linq;

using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models.Implementations;

using ReactiveUI;


namespace BeatSaberModManager.Localisation
{
    public class LanguageSwitcher : ReactiveObject
    {
        public LanguageSwitcher(Settings settings)
        {
            Languages = _supportedLanguageCodes.Select(LoadLanguage).ToArray();
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(x => x.SelectedLanguage).WhereNotNull();
            selectedLanguageObservable.Subscribe(l => Application.Current.Resources.MergedDictionaries[0] = l.ResourceProvider);
            selectedLanguageObservable.Subscribe(l => settings.LanguageCode = l.CultureInfo.Name);
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == settings.LanguageCode) ??
                               Languages.FirstOrDefault(x => x.CultureInfo.Name == CultureInfo.CurrentCulture.Name) ??
                               Languages.First();
        }

        public Language[] Languages { get; }

        private Language _selectedLanguage = null!;
        public Language SelectedLanguage
        {
            get => _selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
        }

        private static Language LoadLanguage(string languageCode)
        {
            ResourceInclude resourceInclude = new() { Source = new Uri($"avares://{nameof(BeatSaberModManager)}/Resources/Localisation/{languageCode}.axaml") };
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            return new Language(cultureInfo, resourceInclude);
        }

        private static readonly string[] _supportedLanguageCodes = { "de", "en" };
    }
}