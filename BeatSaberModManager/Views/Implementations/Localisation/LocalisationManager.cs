using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Views.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Localisation
{
    public class LocalisationManager : ReactiveObject, ILocalisationManager
    {
        private readonly AppSettings _appSettings;

        public LocalisationManager(ISettings<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            Languages = _supportedLanguageCodes.Select(LoadLanguage).ToArray();
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == _appSettings.LanguageCode) ??
                               Languages.FirstOrDefault(x => x.CultureInfo.Name == CultureInfo.CurrentCulture.Name) ??
                               Languages.First();
        }

        public ILanguage[] Languages { get; }

        private ILanguage _selectedLanguage = null!;
        public ILanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
        }

        public void Initialize()
        {
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(x => x.SelectedLanguage).WhereNotNull().Cast<Language>();
            selectedLanguageObservable.Subscribe(l => Application.Current.Resources.MergedDictionaries[0] = l.ResourceProvider);
            selectedLanguageObservable.Subscribe(l => _appSettings.LanguageCode = l.CultureInfo.Name);
        }

        private static ILanguage LoadLanguage(string languageCode)
        {
            ResourceInclude resourceInclude = new() { Source = new Uri($"avares://{nameof(BeatSaberModManager)}/Resources/Localisation/{languageCode}.axaml") };
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            return new Language(cultureInfo, resourceInclude);
        }

        private static readonly string[] _supportedLanguageCodes = { "de", "en" };
    }
}