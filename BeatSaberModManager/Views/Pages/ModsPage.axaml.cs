using System;
using System.Collections;
using System.Reactive.Linq;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using ReactiveUI;


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
            ViewModel.WhenAnyValue(static x => x.GridItems)
                .WhereNotNull()
                .Select(static x => new DataGridCollectionView(x.Values)
                {
                    GroupDescriptions =
                    {
                        new DataGridPathGroupDescription(
                            $"{nameof(ModGridItemViewModel.AvailableMod)}.{nameof(ModGridItemViewModel.AvailableMod.Category)}")
                    }
                })
                .Do(static x => x.MoveCurrentTo(null))
                .BindTo<DataGridCollectionView, DataGrid, IEnumerable>(ModsDataGrid, static x => x.Items);
            ViewModel.WhenAnyValue(static x => x.IsSearchEnabled, static x => x.SearchQuery)
                .Where(x => x.Item2 is not null && ModsDataGrid.Items is DataGridCollectionView)
                .Select(static x => new Func<object, bool>(o => Filter(x.Item1, x.Item2, o)))
                .Subscribe(x => ((DataGridCollectionView)ModsDataGrid.Items).Filter = x);
            ViewModel.WhenAnyValue(static x => x.IsSearchEnabled)
                .Where(static x => x)
                .Subscribe(_ => SearchTextBox.Focus());
        }

        private static bool Filter(bool enabled, string? query, object o) =>
            !enabled || o is not ModGridItemViewModel gridItem || string.IsNullOrWhiteSpace(query) ||
            gridItem.AvailableMod.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
