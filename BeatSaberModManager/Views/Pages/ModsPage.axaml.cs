using System;
using System.Collections;
using System.Reactive.Linq;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Pages
{
    public partial class ModsPage : ReactiveUserControl<ModsViewModel>
    {
        public ModsPage() { }

        [ActivatorUtilitiesConstructor]
        public ModsPage(ModsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.WhenAnyValue(x => x.GridItems)
                .WhereNotNull()
                .Select(x => new DataGridCollectionView(x.Values) { GroupDescriptions = { new DataGridPathGroupDescription($"{nameof(ModGridItemViewModel.AvailableMod)}.{nameof(ModGridItemViewModel.AvailableMod.Category)}") } })
                .Do(x => x.MoveCurrentTo(null))
                .BindTo<DataGridCollectionView, DataGrid, IEnumerable>(ModsDataGrid, x => x.Items);
            ViewModel.WhenAnyValue(x => x.IsSearchEnabled, x => x.SearchQuery)
                .Where(x => x.Item2 is not null && ModsDataGrid.Items is DataGridCollectionView)
                .Select(x => (object o) => Filter(x.Item1, x.Item2!, o))
                .Subscribe(x => ((DataGridCollectionView)ModsDataGrid.Items).Filter = x);
        }

        private static bool Filter(bool enabled, string query, object o) =>
            !enabled || o is not ModGridItemViewModel gridItem || string.IsNullOrWhiteSpace(query) || gridItem.AvailableMod.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}