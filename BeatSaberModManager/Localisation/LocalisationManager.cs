using System;
using System.Globalization;
using System.Linq;

using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models.Implementations.Settings;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.Localisation
{
    public class LocalisationManager : ReactiveObject
    {
        private readonly SettingsStore _settingsStore;

        public LocalisationManager(IOptions<SettingsStore> settingsStore)
        {
            _settingsStore = settingsStore.Value;
            Languages = _supportedLanguageCodes.Select(LoadLanguage).ToArray();
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == _settingsStore.LanguageCode) ??
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

        public void Initialize(Application application)
        {
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(x => x.SelectedLanguage).WhereNotNull();
            selectedLanguageObservable.Subscribe(l => application.Resources.MergedDictionaries[0] = l.ResourceProvider);
            selectedLanguageObservable.Subscribe(l => _settingsStore.LanguageCode = l.CultureInfo.Name);
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