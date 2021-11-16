using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;
using BeatSaberModManager.Views.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class ModsPage : ReactiveUserControl<ModsViewModel>, IPage
    {
        private readonly IInstallDirValidator _installDirValidator = null!;
        private readonly DataGridCollectionView _dataGridCollection = null!;

        public ModsPage() { }

        [ActivatorUtilitiesConstructor]
        public ModsPage(ModsViewModel modsViewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator)
        {
            InitializeComponent();
            ViewModel = modsViewModel;
            _installDirValidator = installDirValidator;
            _dataGridCollection = new DataGridCollectionView(ViewModel.GridItems.Values);
            _dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ReactiveCommand<string?, Unit> refreshCommand = ReactiveCommand.CreateFromTask<string?>(InitializeDataGridAsync);
            appSettings.Value.InstallDir.Changed.InvokeCommand(refreshCommand);
        }

        private async Task InitializeDataGridAsync(string? installDir)
        {
            if (!_installDirValidator.ValidateInstallDir(installDir)) return;
            await ViewModel!.InitializeDataGridAsync();
            ModsDataGrid.Items = _dataGridCollection;
        }
    }
}