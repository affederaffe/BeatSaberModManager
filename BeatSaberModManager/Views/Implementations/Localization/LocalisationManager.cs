using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models.Implementations.Settings;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Localization
{
    public class LocalizationManager : ReactiveObject
    {
        private readonly IOptions<AppSettings> _appSettings;

        public LocalizationManager(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            Languages = _supportedLanguageCodes.Select(LoadLanguage).ToArray();
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == _appSettings.Value.LanguageCode) ??
                               Languages.FirstOrDefault(x => x.CultureInfo.Name == CultureInfo.CurrentCulture.Name) ??
                               Languages[0];
        }

        public IReadOnlyList<Language> Languages { get; }

        private Language _selectedLanguage = null!;
        public Language SelectedLanguage
        {
            get => _selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
        }

        public void Initialize(Action<Language> applyLanguage)
        {
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(x => x.SelectedLanguage).OfType<Language>();
            selectedLanguageObservable.Subscribe(applyLanguage);
            selectedLanguageObservable.Subscribe(l => _appSettings.Value.LanguageCode = l.CultureInfo.Name);
        }

        private static Language LoadLanguage(string languageCode)
        {
            ResourceInclude resourceInclude = new() { Source = new Uri($"avares://{nameof(BeatSaberModManager)}/Resources/Localisation/{languageCode}.axaml") };
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            return new Language(cultureInfo, resourceInclude);
        }

        private static readonly string[] _supportedLanguageCodes = { "en", "de" };
    }
}