using System;
using System.IO;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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
        public SettingsPage(SettingsViewModel viewModel, Window window, LocalizationManager localizationManager, ThemeManager themeManager, IInstallDirValidator installDirValidator)
        {
            InitializeComponent();
            LanguagesComboBox.DataContext = localizationManager;
            ThemesComboBox.DataContext = themeManager;
            ViewModel = viewModel;
            SelectInstallFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()))
                .Where(static x => x.Count > 0)
                .Select(static x => x[0].TryGetUri(out Uri? uri) ? uri : null)
                .WhereNotNull()
                .Select(static x => x.AbsolutePath)
                .Where(installDirValidator.ValidateInstallDir)
                .BindTo(viewModel, static x => x.AppSettings.Value.InstallDir);
            SelectThemesFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()))
                .Where(static x => x?.Count is > 0)
                .Select(static x => x![0].TryGetUri(out Uri? uri) ? uri : null)
                .WhereNotNull()
                .Select(static x => x.AbsolutePath)
                .Where(Directory.Exists)
                .BindTo(viewModel, static x => x.AppSettings.Value.ThemesDir);
        }
    }
}
