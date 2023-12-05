﻿using System.Collections.Generic;
using System.Globalization;


namespace BeatSaberModManager.Models.Implementations.Settings
{
    /// <summary>
    /// Application-wide settings.
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>
        /// The index of the tab that was last open.
        /// </summary>
        public int TabIndex { get; set; }

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
        /// True if selected mods should be saved and restored on restart, false otherwise.
        /// </summary>
        public bool SaveSelectedMods { get; set; } = true;

        /// <summary>
        /// The game's installation directory.
        /// </summary>
        public string? InstallDir { get; set; }

        /// <summary>
        /// A collection of all selected mods.
        /// </summary>
        public HashSet<string> SelectedMods => _selectedMods ??= [];
        private HashSet<string>? _selectedMods;
    }
}
