namespace BeatSaberModManager.Models.Implementations.Settings
{
    public class SettingsStore
    {
        public string? InstallDir { get; set; }
        public string? VRPlatform { get; set; }
        public string? ThemesDir { get; set; }
        public string? ThemeName { get; set; }
        public string? LanguageCode { get; set; }
        public int LastSelectedIndex { get; set; }
    }
}