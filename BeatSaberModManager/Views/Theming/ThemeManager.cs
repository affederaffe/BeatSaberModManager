using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Styling;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.Views.Theming
{
    /// <summary>
    /// Load and apply internal and external <see cref="Theme"/>s.
    /// </summary>
    public class ThemeManager : ReactiveObject
    {
        private readonly ISettings<AppSettings> _appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeManager"/> class.
        /// </summary>
        public ThemeManager(ISettings<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            Themes = new Theme[]
            {
                new("Themes:System", ThemeVariant.Default),
                new("Themes:Light", ThemeVariant.Light),
                new("Themes:Dark", ThemeVariant.Dark)
            };

            _selectedTheme = Themes.FirstOrDefault(x => x.Name == appSettings.Value.ThemeName) ?? Themes[0];
        }

        /// <summary>
        /// A collection of all available <see cref="Theme"/>s.
        /// </summary>
        public IReadOnlyList<Theme> Themes { get; }

        /// <summary>
        /// The currently selected <see cref="Theme"/>.
        /// </summary>
        public Theme SelectedTheme
        {
            get => _selectedTheme!;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        private Theme? _selectedTheme;

        /// <summary>
        /// Initializes theming for an <see cref="Application"/>.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to style.</param>
        public void Initialize(Application application)
        {
            this.WhenAnyValue(static x => x.SelectedTheme).Subscribe(x =>
            {
                application.RequestedThemeVariant = x.ThemeVariant;
                _appSettings.Value.ThemeName = x.Name;
            });
        }

    }
}
