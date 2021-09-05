using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class ModsView : ReactiveUserControl<ModsViewModel>
    {
        private readonly DataGridCollectionView _dataGridCollection = null!;

        public ModsView() { }

        [ActivatorUtilitiesConstructor]
        public ModsView(ModsViewModel modsViewModel, OptionsViewModel optionsViewModel)
        {
            InitializeComponent();
            ViewModel = modsViewModel;
            _dataGridCollection = new DataGridCollectionView(ViewModel.GridItems);
            _dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ReactiveCommand<string?, Unit> refreshCommand = ReactiveCommand.CreateFromTask<string?>(_ => RefreshDataGridAsync());
            optionsViewModel.WhenAnyValue(x => x.InstallDir).InvokeCommand(refreshCommand);
        }

        private async Task RefreshDataGridAsync()
        {
            await Task.Run(ViewModel!.RefreshDataGridAsync);
            ModsDataGrid.Items = _dataGridCollection;
        }
    }
}