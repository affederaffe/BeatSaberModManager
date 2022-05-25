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
        private readonly FluentThemeBase _fluentThemeBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeManager"/> class.
        /// </summary>
        public ThemeManager(ISettings<AppSettings> appSettings)
        {
            _fluentThemeBase = new FluentThemeBase(null!);
            Themes = new ObservableCollection<Theme>
            {
                new("Fluent Light", _fluentThemeBase.FluentLight),
                new("Fluent Dark", _fluentThemeBase.FluentDark)
            };

            _buildInThemesCount = Themes.Count;
            _selectedTheme = Themes.FirstOrDefault(x => x.Name == appSettings.Value.ThemeName) ?? Themes[0];
            ReactiveCommand<string, Unit> reloadThemesCommand = ReactiveCommand.CreateFromTask<string>(ReloadExternalThemesAsync);
            appSettings.Value.WhenAnyValue(static x => x.ThemesDir).Where(Directory.Exists).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(reloadThemesCommand!);
            IObservable<Theme> selectedThemeObservable = this.WhenAnyValue(static x => x.SelectedTheme);
            selectedThemeObservable.Subscribe(t => _fluentThemeBase.Style = t.Style);
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

        /// <summary>
        /// Inserts the <see cref="FluentThemeBase"/> into the <see cref="Application"/>'s <see cref="Application.Styles"/>.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to style.</param>
        public void Initialize(Application application)
        {
            application.Styles.Insert(0, _fluentThemeBase);
        }

        private async Task ReloadExternalThemesAsync(string path)
        {
            for (int i = _buildInThemesCount; i < Themes.Count; i++)
                Themes.RemoveAt(i);
            foreach (string file in Directory.EnumerateFiles(path, "*.axaml"))
            {
                Theme? theme = await LoadThemeAsync(file);
                if (theme is null) continue;
                Themes.Add(theme);
            }
        }

        private static async Task<Theme?> LoadThemeAsync(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string dir = Path.GetDirectoryName(filePath)!;
            string xaml = await File.ReadAllTextAsync(filePath);
            return AvaloniaUtils.TryParse<IStyle>(xaml, dir, out IStyle? style) ? new Theme(name, style) : null;
        }
    }
}