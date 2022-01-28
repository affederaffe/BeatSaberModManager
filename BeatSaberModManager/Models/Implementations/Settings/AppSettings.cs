using System.Collections.Generic;

using ReactiveUI;


namespace BeatSaberModManager.Models.Implementations.Settings
{
    public class AppSettings : ReactiveObject
    {
        public int LastTabIndex { get; set; }

        public string? ThemeName { get; set; }

        public string? LanguageCode { get; set; }

        public string? PlatformType { get; set; }

        public bool ForceReinstallMods { get; set; }

        private string? _installDir;
        public string? InstallDir
        {
            get => _installDir;
            set => this.RaiseAndSetIfChanged(ref _installDir, value);
        }

        private string? _themesDir;
        public string? ThemesDir
        {
            get => _themesDir;
            set => this.RaiseAndSetIfChanged(ref _themesDir, value);
        }

        private HashSet<string>? _selectedMods;
        public HashSet<string> SelectedMods
        {
            get => _selectedMods ??= new HashSet<string>();
            set => _selectedMods = value;
        }
    }
}