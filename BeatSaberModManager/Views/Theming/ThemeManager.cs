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
using BeatSaberModManager.Utils;

using Microsoft.Extensions.Options;

using ReactiveUI;


namespace BeatSaberModManager.Views.Theming
{
    public class ThemeManager : ReactiveObject
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly int _buildInThemesCount;

        public ThemeManager(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            Themes = new ObservableCollection<Theme>
            {
                new("Fluent Light", new StyleInclude((null as Uri)!) { Source = new Uri("avares://BeatSaberModManager/Resources/Styles/FluentLight.axaml") }),
                new("Fluent Dark", new StyleInclude((null as Uri)!) { Source = new Uri("avares://BeatSaberModManager/Resources/Styles/FluentDark.axaml") })
            };

            _buildInThemesCount = Themes.Count;
            _selectedTheme = Themes.FirstOrDefault(x => x.Name == _appSettings.Value.ThemeName) ?? Themes[0];
        }

        public ObservableCollection<Theme> Themes { get; }

        private Theme _selectedTheme;
        public Theme SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        public void Initialize(Action<Theme> applyTheme)
        {
            ReactiveCommand<string, Unit> reloadThemesCommand = ReactiveCommand.CreateFromTask<string>(ReloadExternalThemes);
            _appSettings.Value.ThemesDir.Changed.Where(Directory.Exists).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(reloadThemesCommand!);
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(static x => x.SelectedTheme).OfType<Theme>();
            selectedThemeObservable.Subscribe(applyTheme);
            selectedThemeObservable.Subscribe(t => _appSettings.Value.ThemeName = t.Name);
        }

        private async Task ReloadExternalThemes(string path)
        {
            for (int i = _buildInThemesCount; i < Themes.Count; i++)
                Themes.RemoveAt(i);
            foreach (string file in Directory.EnumerateFiles(path, "*.axaml"))
            {
                Theme? theme = await LoadTheme(file);
                if (theme is null) continue;
                Themes.Add(theme);
            }
        }

        private static async Task<Theme?> LoadTheme(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string dir = Path.GetDirectoryName(filePath)!;
            string xaml = await File.ReadAllTextAsync(filePath);
            return AvaloniaUtils.TryParse<IStyle>(xaml, dir, out IStyle? style) ? new Theme(name, style) : null;
        }
    }
}