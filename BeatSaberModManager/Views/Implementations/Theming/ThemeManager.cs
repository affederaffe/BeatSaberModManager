using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Views.Interfaces;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Theming
{
    public class ThemeManager : ReactiveObject, IThemeManager
    {
        private readonly AppSettings _appSettings;
        private readonly int _buildInThemesCount;

        public ThemeManager(ISettings<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            Themes = new ObservableCollection<ITheme>
            {
                LoadBuildInTheme("Default Light", "avares://Avalonia.Themes.Fluent/Accents/BaseLight.xaml", "avares://Avalonia.Themes.Default/DefaultTheme.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Default.xaml", "avares://Avalonia.Themes.Default/Accents/BaseLight.xaml"),
                LoadBuildInTheme("Default Dark", "avares://Avalonia.Themes.Fluent/Accents/BaseDark.xaml", "avares://Avalonia.Themes.Default/DefaultTheme.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Default.xaml", "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"),
                LoadBuildInTheme("Fluent Light", "avares://Avalonia.Themes.Fluent/FluentLight.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml"),
                LoadBuildInTheme("Fluent Dark", "avares://Avalonia.Themes.Fluent/FluentDark.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            };

            _buildInThemesCount = Themes.Count;
            SelectedTheme = Themes.FirstOrDefault(x => x.Name == _appSettings.ThemeName) ?? Themes.Last();
        }

        public ObservableCollection<ITheme> Themes { get; }

        private ITheme _selectedTheme = null!;
        public ITheme SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        public void Initialize(Action<ITheme> applyTheme)
        {
            _appSettings.ThemesDir.Changed.Subscribe(ReloadExternalThemes);
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(x => x.SelectedTheme).OfType<Theme>();
            selectedThemeObservable.Subscribe(applyTheme);
            selectedThemeObservable.Subscribe(t => _appSettings.ThemeName = t.Name);
        }

        public void ReloadExternalThemes(string? path)
        {
            if (!Directory.Exists(path)) return;
            for (int i = _buildInThemesCount; i < Themes.Count; i++)
                Themes.RemoveAt(i);
            IEnumerable<ITheme> themes = Directory.EnumerateFiles(path, "*.xaml").Select(LoadTheme).Where(x => x is not null)!;
            foreach (ITheme theme in themes)
                Themes.Add(theme);
        }

        private static ITheme? LoadTheme(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string name = Path.GetFileNameWithoutExtension(filePath);
            string xaml = File.ReadAllText(filePath);
            IStyle style = AvaloniaRuntimeXamlLoader.Parse<Style>(xaml);
            return new Theme(name, style);
        }

        private static Theme LoadBuildInTheme(string name, params string[] uris)
        {
            Styles styles = new();
            foreach (string uri in uris)
                styles.Add(new StyleInclude((null as Uri)!) { Source = new Uri(uri) });
            return new Theme(name, styles);
        }
    }
}