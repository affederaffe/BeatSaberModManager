using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class ModsPage : ReactiveUserControl<ModsViewModel>, IPage
    {
        private readonly DataGridCollectionView _dataGridCollection = null!;

        public ModsPage() { }

        [ActivatorUtilitiesConstructor]
        public ModsPage(ModsViewModel modsViewModel, OptionsViewModel optionsViewModel)
        {
            InitializeComponent();
            ViewModel = modsViewModel;
            _dataGridCollection = new DataGridCollectionView(ViewModel.GridItems.Values);
            _dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ReactiveCommand<string?, Unit> refreshCommand = ReactiveCommand.CreateFromTask<string?>(RefreshDataGridAsync);
            optionsViewModel.WhenAnyValue(x => x.InstallDir).InvokeCommand(refreshCommand);
        }

        private async Task RefreshDataGridAsync(string? _)
        {
            await ViewModel!.RefreshDataGridAsync();
            ModsDataGrid.Items = _dataGridCollection;
        }
    }
}