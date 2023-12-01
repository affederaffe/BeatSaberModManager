using System;

using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;


namespace BeatSaberModManager.Views.Pages
{
    /// <summary>
    /// TODO
    /// </summary>
    public partial class LegacyGameVersionsPage : ReactiveUserControl<LegacyGameVersionsViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public LegacyGameVersionsPage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyGameVersionsPage"/> class.
        /// </summary>
        public LegacyGameVersionsPage(LegacyGameVersionsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            InitializeComponent();
            ViewModel = viewModel;
        }
    }
}
