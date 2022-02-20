using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Styling;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using ReactiveUI;


namespace BeatSaberModManager.Views.Theming
{
    /// <summary>
    /// Load and apply internal and external <see cref="Theme"/>s.
    /// </summary>
    public class ThemeManager : ReactiveObject
    {
        private readonly int _buildInThemesCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeManager"/> class.
        /// </summary>
        public ThemeManager(ISettings<AppSettings> appSettings, Application application)
        {
            FluentTheme fluentTheme = new((null as Uri)!);
            application.Styles.Insert(0, fluentTheme);
            Themes = new ObservableCollection<Theme>
            {
                new("Fluent Light", fluentTheme.FluentLight),
                new("Fluent Dark", fluentTheme.FluentDark)
            };

            _buildInThemesCount = Themes.Count;
            _selectedTheme = Themes.FirstOrDefault(x => x.Name == appSettings.Value.ThemeName) ?? Themes[0];
            ReactiveCommand<string, Unit> reloadThemesCommand = ReactiveCommand.CreateFromTask<string>(ReloadExternalThemes);
            appSettings.Value.WhenAnyValue(static x => x.ThemesDir).Where(Directory.Exists).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(reloadThemesCommand!);
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(static x => x.SelectedTheme);
            selectedThemeObservable.Subscribe(t => fluentTheme.Style = t.Style);
            selectedThemeObservable.Subscribe(t => appSettings.Value.ThemeName = t.Name);
        }

        /// <summary>
        /// A collection of all available <see cref="Theme"/>s.
        /// </summary>
        public ObservableCollection<Theme> Themes { get; }

        /// <summary>
        /// The currently selected <see cref="Theme"/>.
        /// </summary>
        public Theme SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        private Theme _selectedTheme;

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