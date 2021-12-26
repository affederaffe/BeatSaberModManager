using System;
using System.Collections.Generic;

using Avalonia.Collections;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


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
            ViewModel.WhenAnyValue(x => x.GridItems).WhereNotNull().Subscribe(InitializeDataGrid);
        }

        private void InitializeDataGrid(IEnumerable<ModGridItemViewModel> gridItems)
        {
            DataGridCollectionView dataGridCollection = new(gridItems);
            dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ModsDataGrid.Items = dataGridCollection;
            dataGridCollection.MoveCurrentTo(null);
        }

        public static readonly FuncValueConverter<bool, IBrush> IsUpToDateColorConverter = new(x => x ? Brushes.Green : Brushes.Red);
    }
}