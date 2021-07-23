using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.Views
{
    public class IntroView : ReactiveUserControl<IntroViewModel>
    {
        public IntroView()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<IntroViewModel>();
        }
    }
}