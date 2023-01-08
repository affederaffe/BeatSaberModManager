using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

using Avalonia;
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
            DataGridCollectionView dataGridCollectionView = new(viewModel.GridItems)
            {
                GroupDescriptions =
                {
                    new DataGridFuncGroupDescription<ModGridItemViewModel, string>(static x => x.AvailableMod.Category)
                },
                SortDescriptions =
                {
                    new DataGridComparerSortDescription(Comparer<ModGridItemViewModel>.Create(static (x, y) => x.AvailableMod.IsRequired.CompareTo(y.AvailableMod.IsRequired)), ListSortDirection.Descending),
                    new DataGridComparerSortDescription(Comparer<ModGridItemViewModel>.Create(static (x, y) => string.CompareOrdinal(x.AvailableMod.Category, y.AvailableMod.Category)), ListSortDirection.Ascending),
                    new DataGridComparerSortDescription(Comparer<ModGridItemViewModel>.Create(static (x, y) => string.CompareOrdinal(x.AvailableMod.Name, y.AvailableMod.Name)), ListSortDirection.Ascending)
                }
            };

            ModsDataGrid.Items = dataGridCollectionView;
            ModsDataGrid.GetObservable(SearchableDataGrid.IsSearchEnabledProperty)
                .CombineLatest(ModsDataGrid.GetObservable(SearchableDataGrid.TextProperty))
                .Where(static x => x.Second is not null)
                .Select(static x => new Func<object, bool>(o => Filter(x.First, x.Second, o)))
                .Subscribe(x => dataGridCollectionView.Filter = x);
        }

        private static bool Filter(bool enabled, string? query, object o) =>
            !enabled || o is not ModGridItemViewModel gridItem || string.IsNullOrWhiteSpace(query) ||
            gridItem.AvailableMod.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            gridItem.AvailableMod.Description.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
