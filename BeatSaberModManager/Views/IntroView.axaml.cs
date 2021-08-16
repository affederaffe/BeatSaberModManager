using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.Views
{
    public partial class IntroView : ReactiveUserControl<IntroViewModel>
    {
        public IntroView()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<IntroViewModel>();
        }
    }
}