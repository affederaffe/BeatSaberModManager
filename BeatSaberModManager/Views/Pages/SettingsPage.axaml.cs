using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Services.Implementations.Progress;
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
        public SettingsPage(SettingsViewModel viewModel, Window window, LocalizationManager localizationManager, ThemeManager themeManager)
        {
            InitializeComponent();
            ViewModel = viewModel;
            LanguagesComboBox.Items = localizationManager.Languages;
            LanguagesComboBox.SelectedItem = localizationManager.SelectedLanguage;
            LanguagesComboBox.GetObservable(SelectingItemsControl.SelectedItemProperty).BindTo(localizationManager, static x => x.SelectedLanguage);
            ThemesComboBox.Items = themeManager.Themes;
            ThemesComboBox.SelectedItem = themeManager.SelectedTheme;
            ThemesComboBox.GetObservable(SelectingItemsControl.SelectedItemProperty).BindTo(themeManager, static x => x.SelectedTheme);
            SelectInstallFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(window))
                .BindTo(ViewModel, static x => x.InstallDir);
            SelectThemesFolderButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFolderDialog().ShowAsync(window))
                .BindTo(ViewModel, static x => x.ThemesDir);
            InstallPlaylistButton.GetObservable(Button.ClickEvent)
                .SelectMany(_ => new OpenFileDialog { AllowMultiple = false, Filters = { new FileDialogFilter { Extensions = { "bplist" }, Name = "BeatSaber Playlist" } } }.ShowAsync(window))
                .Where(static x => x?.Length is 1)
                .Select(static x => x![0])
                .ObserveOn(RxApp.MainThreadScheduler)
                .SelectMany(ViewModel.InstallPlaylistAsync)
                .Select(static x => new ProgressInfo(x ? StatusType.Completed : StatusType.Failed, null))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ViewModel.StatusProgress.Report);
        }
    }
}