using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.DependencyInjection
{
    public static class ViewModelBootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton(() => new AssetInstallWindowViewModel(resolver.GetServices<IAssetProvider>(), (StatusProgress)resolver.GetService<IStatusProgress>()));
            services.RegisterLazySingleton(() => new IntroViewModel());
            services.RegisterLazySingleton(() => new ModsViewModel(resolver.GetService<IModProvider>(), resolver.GetService<IModInstaller>(), resolver.GetService<IModVersionComparer>(), resolver.GetService<IStatusProgress>()));
            services.RegisterLazySingleton(() => new OptionsViewModel(resolver.GetService<ModsViewModel>(), resolver.GetService<Settings>(), resolver.GetService<PlaylistInstaller>(), resolver.GetService<IInstallDirValidator>(), resolver.GetService<IStatusProgress>()));
            services.RegisterLazySingleton(() => new MainWindowViewModel(resolver.GetService<ModsViewModel>(), resolver.GetService<Settings>(), (StatusProgress)resolver.GetService<IStatusProgress>()));
        }
    }
}