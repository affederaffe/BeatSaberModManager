using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Controls;


namespace BeatSaberModManager.Views.Pages
{
    /// <summary>
    /// View for installing and uninstalling mods.
    /// </summary>
    public partial class ModsPage : ReactiveUserControl<ModsViewModel>
    {
        /// <summary>
        /// [Required by Avalonia]
        /// </summary>
        public ModsPage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModsPage"/> class.
        /// </summary>
        public ModsPage(ModsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ModsDataGrid.ItemsSource = new DataGridCollectionView(viewModel.GridItems)
            {
                GroupDescriptions =
                {
                    new DataGridFuncGroupDescription<ModGridItemViewModel, string>(static x => x.AvailableMod.Category)
                }
            };
        }
    }
}
