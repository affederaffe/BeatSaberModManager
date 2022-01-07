using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utilities;
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
                new Theme("Fluent Light", new StyleInclude((null as Uri)!) { Source = new Uri("avares://BeatSaberModManager/Resources/Styles/FluentLight.axaml") }),
                new Theme("Fluent Dark", new StyleInclude((null as Uri)!) { Source = new Uri("avares://BeatSaberModManager/Resources/Styles/FluentDark.axaml") })
            };

            _buildInThemesCount = Themes.Count;
            _selectedTheme = Themes.FirstOrDefault(x => x.Name == _appSettings.Value.ThemeName) ?? Themes[0];
        }

        public ObservableCollection<ITheme> Themes { get; }

        private ITheme _selectedTheme;
        public ITheme SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        public void Initialize(Action<ITheme> applyTheme)
        {
            ReactiveCommand<string, Unit> reloadThemesCommand = ReactiveCommand.CreateFromTask<string>(ReloadExternalThemes);
            _appSettings.Value.ThemesDir.Changed.Where(Directory.Exists).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(reloadThemesCommand!);
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(x => x.SelectedTheme).OfType<Theme>();
            selectedThemeObservable.Subscribe(applyTheme);
            selectedThemeObservable.Subscribe(t => _appSettings.Value.ThemeName = t.Name);
        }

        private async Task ReloadExternalThemes(string path)
        {
            for (int i = _buildInThemesCount; i < Themes.Count; i++)
                Themes.RemoveAt(i);
            foreach (string file in Directory.EnumerateFiles(path, "*.axaml"))
            {
                ITheme? theme = await LoadTheme(file);
                if (theme is null) continue;
                Themes.Add(theme);
            }
        }

        private static async Task<ITheme?> LoadTheme(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string dir = Path.GetDirectoryName(filePath)!;
            string xaml = await File.ReadAllTextAsync(filePath);
            return AvaloniaUtils.TryParse<IStyle>(xaml, dir, out IStyle? style) ? new Theme(name, style) : null;
        }
    }
}