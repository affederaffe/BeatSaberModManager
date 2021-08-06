using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.DependencyInjection
{
    public static class ViewModelBootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton(() => new AssetInstallWindowViewModel(resolver.GetServices<IAssetProvider>()));
            services.RegisterLazySingleton(() => new IntroViewModel());
            services.RegisterLazySingleton(() => new ModsViewModel(resolver.GetService<Settings>(), resolver.GetService<IModProvider>(), resolver.GetService<IModInstaller>(), resolver.GetService<IModVersionComparer>()));
            services.RegisterLazySingleton(() => new OptionsViewModel(resolver.GetService<ModsViewModel>(), resolver.GetService<Settings>(), resolver.GetService<PlaylistInstaller>(), resolver.GetService<IInstallDirValidator>()));
            services.RegisterLazySingleton(() => new MainWindowViewModel(resolver.GetService<ModsViewModel>()));
        }
    }
}