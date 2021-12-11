using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class ModsPage : ReactiveUserControl<ModsViewModel>
    {
        public ModsPage() { }

        [ActivatorUtilitiesConstructor]
        public ModsPage(ModsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            if (ViewModel.GridItems is null) return;
            DataGridCollectionView dataGridCollection = new(ViewModel.GridItems.Values);
            dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ModsDataGrid.Items = dataGridCollection;
            dataGridCollection.MoveCurrentTo(null);
        }
    }
}