using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Styling;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;
using BeatSaberModManager.ViewModels;

using ReactiveUI;


namespace BeatSaberModManager.Views.Theming
{
    /// <summary>
    /// Load and apply internal and external <see cref="Theme"/>s.
    /// </summary>
    public class ThemeManager : ReactiveObject
    {
        private readonly SettingsViewModel _settingsViewModel;
        private readonly FluentThemeBase _fluentThemeBase;
        private readonly int _buildInThemesCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeManager"/> class.
        /// </summary>
        public ThemeManager(ISettings<AppSettings> appSettings, SettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
            _fluentThemeBase = new FluentThemeBase(null!);
            Themes = new ObservableCollection<Theme>();
            Themes.CollectionChanged += (_, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Add || args.NewItems?.Count != 1) return;
                Theme theme = (args.NewItems[0] as Theme)!;
                if (theme.Name != appSettings.Value.ThemeName) return;
                _fluentThemeBase.Style = theme.Style;
                SelectedTheme = theme;
            };

            Theme fluentLight = new("Fluent Light", _fluentThemeBase.FluentLight);
            Theme fluentDark = new("Fluent Dark", _fluentThemeBase.FluentDark);
            _selectedTheme = fluentLight;
            Themes.Add(fluentLight);
            Themes.Add(fluentDark);
            _buildInThemesCount = Themes.Count;

            this.WhenAnyValue(static x => x.SelectedTheme).Skip(1).WhereNotNull().Subscribe(x =>
            {
                _fluentThemeBase.Style = x.Style;
                appSettings.Value.ThemeName = x.Name;
            });
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
            ReactiveCommand<string, Unit> reloadThemesCommand = ReactiveCommand.CreateFromTask<string>(ReloadExternalThemesAsync);
            _settingsViewModel.ValidatedThemesDirObservable.ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(reloadThemesCommand);
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
