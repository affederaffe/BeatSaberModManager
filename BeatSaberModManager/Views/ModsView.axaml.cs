using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.Views
{
    public partial class ModsView : ReactiveUserControl<ModsViewModel>
    {
        private readonly DataGridCollectionView _dataGridCollection;
        
        public ModsView()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<ModsViewModel>();
            OptionsViewModel optionsViewModel = Locator.Current.GetService<OptionsViewModel>();
            _dataGridCollection = new DataGridCollectionView(ViewModel!.GridItems);
            _dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ReactiveCommand<string?, Unit> refreshCommand = ReactiveCommand.CreateFromTask<string?>(_ => RefreshDataGridAsync());
            optionsViewModel.WhenAnyValue(x => x.ThemesDir).InvokeCommand(refreshCommand);
        }

        private async Task RefreshDataGridAsync()
        {
            await ViewModel!.RefreshDataGridAsync();
            ModsDataGrid.Items = _dataGridCollection;
        }
    }
}