using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

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
        private readonly ISettings<AppSettings> _appSettings;
        private readonly int _buildInThemesCount;

        public ThemeManager(ISettings<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            Themes = new ObservableCollection<ITheme>
            {
                LoadBuildInTheme("Fluent Light", "avares://Avalonia.Themes.Fluent/FluentLight.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml"),
                LoadBuildInTheme("Fluent Dark", "avares://Avalonia.Themes.Fluent/FluentDark.xaml", "avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            };

            _buildInThemesCount = Themes.Count;
            SelectedTheme = Themes.FirstOrDefault(x => x.Name == _appSettings.Value.ThemeName) ?? Themes.Last();
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
            ReactiveCommand<string, Unit> reloadThemesCommand = ReactiveCommand.CreateFromTask<string>(ReloadExternalThemes);
            _appSettings.Value.ThemesDir.Changed.Where(Directory.Exists).InvokeCommand(reloadThemesCommand!);
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(x => x.SelectedTheme).OfType<Theme>();
            selectedThemeObservable.Subscribe(applyTheme);
            selectedThemeObservable.Subscribe(t => _appSettings.Value.ThemeName = t.Name);
        }

        private async Task ReloadExternalThemes(string path)
        {
            for (int i = _buildInThemesCount; i < Themes.Count; i++)
                Themes.RemoveAt(i);
            IEnumerable<Task<ITheme>> themes = Directory.EnumerateFiles(path, "*.axaml").Select(LoadTheme);
            foreach (Task<ITheme> theme in themes)
                Themes.Add(await theme);
        }

        private static async Task<ITheme> LoadTheme(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string xaml = await File.ReadAllTextAsync(filePath);
            IStyle style = AvaloniaRuntimeXamlLoader.Parse<Styles>(xaml);
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