using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia;
using Avalonia.Markup.Xaml.Styling;

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
        private readonly ISettings<AppSettings> _appSettings;

        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationManager"/> class.
        /// </summary>
        public LocalizationManager(ISettings<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// A collection of all available <see cref="Language"/>s.
        /// </summary>
        public IReadOnlyList<Language>? Languages { get; private set; }

        /// <summary>
        /// The currently selected <see cref="Language"/>.
        /// </summary>
        public Language SelectedLanguage
        {
            get => _selectedLanguage!;
            set => _appSettings.Value.LanguageCode = this.RaiseAndSetIfChanged(ref _selectedLanguage!, value).CultureInfo.Name;
        }

        private Language? _selectedLanguage;

        /// <summary>
        /// Start listening to changes of <see cref="SelectedLanguage"/> and apply it to the given <see cref="Application"/>'s <see cref="Application.Resources"/>.
        /// </summary>
        /// <param name="application">The application to apply the <see cref="Language"/> to.</param>
        public void Initialize(Application application)
        {
            Languages = _supportedLanguageCodes.Select(l => LoadLanguage(application, l)).ToArray();
            _selectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Name == _appSettings.Value.LanguageCode) ??
                                Languages.FirstOrDefault(static x => x.CultureInfo.Name == CultureInfo.CurrentCulture.Name) ??
                                Languages[0];
            IObservable<Language> selectedLanguageObservable = this.WhenAnyValue(static x => x.SelectedLanguage);
            selectedLanguageObservable.Subscribe(l =>
            {
                if (!_isInitialized)
                {
                    application.Resources.MergedDictionaries.Insert(0, l.ResourceProvider);
                    _isInitialized = true;
                }
                else
                {
                    application.Resources.MergedDictionaries[0] = l.ResourceProvider;
                }
            });
        }

        private static Language LoadLanguage(Application application, string languageCode)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageCode);
            return new Language(cultureInfo, (application.Resources[languageCode] as MergeResourceInclude)!);
        }

        private static readonly string[] _supportedLanguageCodes = ["en", "de"];
    }
}
