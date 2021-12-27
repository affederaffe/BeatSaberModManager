using System.Reactive.Linq;

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
            ViewModel.WhenAnyValue(x => x.GridItems)
                .WhereNotNull()
                .Select(x => new DataGridCollectionView(x) { GroupDescriptions = { new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)) }})
                .Do(x => x.MoveCurrentTo(null))
                .BindTo(ModsDataGrid, x => x.Items);
        }

        public static readonly FuncValueConverter<bool, IBrush> IsUpToDateColorConverter = new(x => x ? Brushes.Green : Brushes.Red);
    }
}