using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Theming
{
    public class ThemeManager : ReactiveObject, IThemeManager
    {
        private readonly SettingsStore _settingsStore;
        private readonly int _buildInThemesCount;

        public ThemeManager(IOptions<SettingsStore> settingsStore)
        {
            _settingsStore = settingsStore.Value;
            Themes = new ObservableCollection<ITheme>
            {
                LoadBuildInTheme("Default Light", "avares://Avalonia.Themes.Default/DefaultTheme.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Default.xaml", "avares://Avalonia.Themes.Default/Accents/BaseLight.xaml", "avares://BeatSaberModManager/Resources/Styles/DefaultLight.axaml", "avares://BeatSaberModManager/Resources/Styles/DefaultProgressRing.axaml"),
                LoadBuildInTheme("Default Dark", "avares://Avalonia.Themes.Default/DefaultTheme.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Default.xaml", "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml", "avares://BeatSaberModManager/Resources/Styles/DefaultDark.axaml", "avares://BeatSaberModManager/Resources/Styles/DefaultProgressRing.axaml"),
                LoadBuildInTheme("Fluent Light", "avares://Avalonia.Themes.Fluent/FluentLight.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", "avares://BeatSaberModManager/Resources/Styles/FluentProgressRing.axaml"),
                LoadBuildInTheme("Fluent Dark", "avares://Avalonia.Themes.Fluent/FluentDark.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", "avares://BeatSaberModManager/Resources/Styles/FluentProgressRing.axaml")
            };

            _buildInThemesCount = Themes.Count;
            SelectedTheme = Themes.FirstOrDefault(x => x.Name == _settingsStore.ThemeName) ?? Themes.First();
        }

        public ObservableCollection<ITheme> Themes { get; }

        private ITheme _selectedTheme = null!;
        public ITheme SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        public void Initialize()
        {
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(x => x.SelectedTheme).WhereNotNull().Cast<Theme>();
            selectedThemeObservable.Subscribe(t => Application.Current.Styles[0] = t.Style);
            selectedThemeObservable.Subscribe(t => _settingsStore.ThemeName = t.Name);
        }

        public void ReloadExternalThemes()
        {
            if (!Directory.Exists(_settingsStore.ThemesDir)) return;
            for (int i = _buildInThemesCount; i < Themes.Count; i++)
                Themes.RemoveAt(i);
            IEnumerable<ITheme> themes = Directory.EnumerateFiles(_settingsStore.ThemesDir, "*.xaml").Select(LoadTheme).Where(x => x is not null)!;
            foreach (ITheme theme in themes)
                Themes.Add(theme);
        }

        private static ITheme? LoadTheme(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string name = Path.GetFileNameWithoutExtension(filePath);
            string xaml = File.ReadAllText(filePath);
            IStyle style = AvaloniaRuntimeXamlLoader.Parse<IStyle>(xaml);
            return new Theme(name, style);
        }

        private static Theme LoadBuildInTheme(string name, params string[] uris)
        {
            Styles styles = new();
            foreach (string uri in uris)
            {
                styles.Add(new StyleInclude((Uri?)null!)
                {
                    Source = new Uri(uri)
                });
            }

            return new Theme(name, styles);
        }
    }
}