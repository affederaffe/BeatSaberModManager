using System.Collections.Generic;
using System.Globalization;

using BeatSaberModManager.Views.Theming;

using ReactiveUI;


namespace BeatSaberModManager.Models.Implementations.Settings
{
    /// <summary>
    /// Application-wide settings.
    /// </summary>
    public class AppSettings : ReactiveObject
    {
        /// <summary>
        /// The index of the tab that was last open.
        /// </summary>
        public int LastTabIndex { get; set; }

        /// <summary>
        /// The name of the theme used.
        /// </summary>
        public string? ThemeName { get; set; }

        /// <summary>
        /// The <see cref="CultureInfo.Name"/> of the language used.
        /// </summary>
        public string? LanguageCode { get; set; }

        /// <summary>
        /// True if already installed mods should be reinstalled, false otherwise.
        /// </summary>
        public bool ForceReinstallMods { get; set; }

        /// <summary>
        /// True if the OneClick installation window should automatically close, false otherwise.
        /// </summary>
        public bool CloseOneClickWindow { get; set; } = true;

        /// <summary>
        /// The game's installation directory.
        /// </summary>
        public string? InstallDir
        {
            get => _installDir;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _installDir;

        /// <summary>
        /// The directory containing additional <see cref="Theme"/>s.
        /// </summary>
        public string? ThemesDir
        {
            get => _themesDir;
            set => this.RaiseAndSetIfChanged(ref _themesDir, value);
        }

        private string? _themesDir;

        /// <summary>
        /// A collection of all selected mods.
        /// </summary>
        public HashSet<string> SelectedMods
        {
            get => _selectedMods ??= new HashSet<string>();
            set => _selectedMods = value;
        }

        private HashSet<string>? _selectedMods;
    }
}