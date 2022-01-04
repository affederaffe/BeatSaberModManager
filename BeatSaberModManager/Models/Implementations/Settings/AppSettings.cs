using System.Collections.Generic;

using BeatSaberModManager.Models.Implementations.Observables;


namespace BeatSaberModManager.Models.Implementations.Settings
{
    public class AppSettings
    {
        public int LastTabIndex { get; set; }

        public string? ThemeName { get; set; }

        public string? LanguageCode { get; set; }

        public bool ForceReinstallMods { get; set; }

        private ObservableVariable<string>? _installDir;
        public ObservableVariable<string> InstallDir
        {
            get => _installDir ??= new ObservableVariable<string>();
            set => _installDir = value;
        }

        private ObservableVariable<string>? _themesDir;
        public ObservableVariable<string> ThemesDir
        {
            get => _themesDir ??= new ObservableVariable<string>();
            set => _themesDir = value;
        }

        private HashSet<string>? _selectedMods;
        public HashSet<string> SelectedMods
        {
            get => _selectedMods ??= new HashSet<string>();
            set => _selectedMods = value;
        }
    }
}