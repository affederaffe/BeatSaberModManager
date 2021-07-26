using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

using ReactiveUI;


namespace BeatSaberModManager.ThemeManagement
{
    public class ThemeSwitcher : ReactiveObject
    {
        private readonly Uri _styleIncludeUri = new("avares://Avalonia.ThemeManager/Styles");

        public ThemeSwitcher(string? directoryPath)
        {
            _themes = new ObservableCollection<Theme>
            {
                LoadBuildInTheme("Default Light", "avares://Avalonia.Themes.Default/DefaultTheme.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Default.xaml", "avares://Avalonia.Themes.Default/Accents/BaseLight.xaml", "avares://BeatSaberModManager/Resources/Styles/DefaultLight.axaml"),
                LoadBuildInTheme("Default Dark", "avares://Avalonia.Themes.Default/DefaultTheme.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Default.xaml", "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml", "avares://BeatSaberModManager/Resources/Styles/DefaultDark.axaml"),
                LoadBuildInTheme("Fluent Light", "avares://Avalonia.Themes.Fluent/FluentLight.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml"),
                LoadBuildInTheme("Fluent Dark", "avares://Avalonia.Themes.Fluent/FluentDark.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            };

            if (Directory.Exists(directoryPath))
            {
                foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.xaml"))
                {
                    Theme? theme = LoadTheme(filePath);
                    if (theme is null) continue;
                    _themes.Add(theme);
                }
            }

            this.WhenAnyValue(x => x.SelectedTheme).Subscribe(b => Application.Current.Styles[0] = b.Style);
        }

        private Theme? _selectedTheme;
        public Theme SelectedTheme
        {
            get => _selectedTheme ??= Themes.First();
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        private ObservableCollection<Theme> _themes;
        public ObservableCollection<Theme> Themes
        {
            get => _themes;
            set => this.RaiseAndSetIfChanged(ref _themes, value);
        }

        public Theme? LoadTheme(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string name = Path.GetFileNameWithoutExtension(filePath);
            string xaml = File.ReadAllText(filePath);
            IStyle style = AvaloniaRuntimeXamlLoader.Parse<IStyle>(xaml);
            return new Theme(name, style);
        }

        private Theme LoadBuildInTheme(string name, params string[] uris)
        {
            Styles styles = new();
            foreach (string uri in uris)
            {
                styles.Add(new StyleInclude(_styleIncludeUri)
                {
                    Source = new Uri(uri)
                });
            }

            return new Theme(name, styles);
        }
    }
}