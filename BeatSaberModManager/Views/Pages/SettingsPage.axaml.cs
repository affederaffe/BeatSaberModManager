﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Localization;
using BeatSaberModManager.Views.Theming;


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
        public SettingsPage(SettingsViewModel viewModel, Window window, LocalizationManager localizationManager, ThemeManager themeManager)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            InitializeComponent();
            LanguagesComboBox.DataContext = localizationManager;
            ThemesComboBox.DataContext = themeManager;
            ViewModel = viewModel;
            viewModel.PickInstallDirInteraction.RegisterHandler(async context => context.SetOutput(await SelectInstallDirAsync(window).ConfigureAwait(false)));
        }

        private static async Task<string?> SelectInstallDirAsync(TopLevel window)
        {
            IReadOnlyList<IStorageFolder> directories = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()).ConfigureAwait(false);
            return directories.Count == 1 ? directories[0].TryGetLocalPath() : null;
        }
    }
}
