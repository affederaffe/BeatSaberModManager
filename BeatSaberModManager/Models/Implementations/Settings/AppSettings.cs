using System.Collections.Generic;


namespace BeatSaberModManager.Models.Implementations.Settings
{
    public class AppSettings
    {
        public string? InstallDir { get; set; }
        public string? VrPlatform { get; set; }
        public string? ThemesDir { get; set; }
        public string? ThemeName { get; set; }
        public string? LanguageCode { get; set; }
        private HashSet<string>? _selectedMods;
        public HashSet<string> SelectedMods
        {
            get => _selectedMods ??= new HashSet<string>();
            set => _selectedMods = value;
        }
    }
}