using System.IO;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Theming;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Pages
{
    public partial class SettingsPage : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsPage() { }

        [ActivatorUtilitiesConstructor]
        public SettingsPage(SettingsViewModel viewModel, Window window, LocalizationManager localizationManager, ThemeManager themeManager, IInstallDirValidator installDirValidator)
        {
            InitializeComponent();
            ViewModel = viewModel;
            LanguagesComboBox.DataContext = localizationManager;
            ThemesComboBox.DataContext = themeManager;
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