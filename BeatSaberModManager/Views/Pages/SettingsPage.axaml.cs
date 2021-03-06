using System.IO;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Theming;

using ReactiveUI;


namespace BeatSaberModManager.Views.Pages
{
    /// <summary>
    /// View for user settings.
    /// </summary>
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public SettingsPage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage(SettingsViewModel viewModel, Window window, LocalizationManager localizationManager, ThemeManager themeManager,
            IInstallDirValidator installDirValidator)
        {
            InitializeComponent();
            LanguagesComboBox.DataContext = localizationManager;
            ThemesComboBox.DataContext = themeManager;
            ViewModel = viewModel;
            SelectInstallFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(window))
                .Where(installDirValidator.ValidateInstallDir)
                .BindTo(viewModel, static x => x.AppSettings.Value.InstallDir);
            SelectThemesFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(window))
                .Where(Directory.Exists)
                .BindTo(viewModel, static x => x.AppSettings.Value.ThemesDir);
        }
    }
}
