using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.ViewModels;

using ReactiveUI;

using Splat;


namespace BeatSaberModManager.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<MainWindowViewModel>();
            Label label = this.FindControl<Label>("ProgressBarLabel");
            ViewModel.WhenAnyValue(x => x.ProgressBarPreTextResourceName, x => x.ProgressBarText)
                     .Select(x => this.TryFindResource(x.Item1, out object? resource) ? $"{resource} {x.Item2}" : x.Item2)
                     .BindTo(label, x => x.Content);
            this.WhenActivated(async _ =>
            {
                OptionsViewModel optionsViewModel = Locator.Current.GetService<OptionsViewModel>();
                if (optionsViewModel.InstallDir is not null) return;
                IInstallDirLocator installDirLocator = Locator.Current.GetService<IInstallDirLocator>();
                if (installDirLocator.TryDetectInstallDir(out string? installDir))
                    optionsViewModel.InstallDir = installDir;
                optionsViewModel.InstallDir ??= await new InstallFolderDialogWindow().ShowDialog<string?>(this);
            });
        }
    }
}