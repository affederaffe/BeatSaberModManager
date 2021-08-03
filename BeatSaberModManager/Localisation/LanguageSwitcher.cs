using System;
using System.Linq;

using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;

using BeatSaberModManager.Models;

using ReactiveUI;


namespace BeatSaberModManager.Localisation
{
    public class LanguageSwitcher : ReactiveObject
    {
        public LanguageSwitcher(Settings settings)
        {
            Languages = new[]
            {
                LoadLanguage("English", "avares://BeatSaberModManager/Resources/Localisation/en.axaml"),
                LoadLanguage("Deutsch", "avares://BeatSaberModManager/Resources/Localisation/de.axaml")
            };

            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(x => x.SelectedLanguage).WhereNotNull();
            selectedLanguageObservable.Subscribe(l => Application.Current.Resources.MergedDictionaries[0] = l.ResourceProvider);
            selectedLanguageObservable.Subscribe(l => settings.LanguageName = l.Name);
        }

        public Language[] Languages { get; }

        private Language? _selectedLanguage;
        public Language? SelectedLanguage
        {
            get => _selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
        }

        public void Initialize(string? lastLanguage)
        {
            SelectedLanguage = Languages.FirstOrDefault(x => x.Name == lastLanguage) ?? Languages.First();
        }

        private static Language LoadLanguage(string name, string uri)
        {
            ResourceInclude resourceInclude = new() { Source = new Uri(uri) };
            return new Language(name, resourceInclude);
        }
    }
}