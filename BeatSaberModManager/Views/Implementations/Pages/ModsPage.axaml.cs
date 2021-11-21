using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations.Pages
{
    public partial class ModsPage : ReactiveUserControl<ModsViewModel>
    {
        private readonly IInstallDirValidator _installDirValidator = null!;

        public ModsPage() { }

        [ActivatorUtilitiesConstructor]
        public ModsPage(ModsViewModel viewModel, ISettings<AppSettings> appSettings, IInstallDirValidator installDirValidator)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _installDirValidator = installDirValidator;
            ReactiveCommand<string?, Unit> refreshCommand = ReactiveCommand.CreateFromTask<string?>(InitializeDataGridAsync);
            appSettings.Value.InstallDir.Changed.InvokeCommand(refreshCommand);
            DataGridCollectionView dataGridCollection = new(ViewModel.GridItems.Values);
            dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            ModsDataGrid.Items = dataGridCollection;
        }

        private async Task InitializeDataGridAsync(string? installDir)
        {
            if (!_installDirValidator.ValidateInstallDir(installDir)) return;
            await ViewModel!.InitializeDataGridAsync();
        }
    }
}