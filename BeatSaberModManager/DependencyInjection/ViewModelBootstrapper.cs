using BeatSaberModManager.Models;
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
            services.RegisterLazySingleton(() => new ModsViewModel(resolver.GetService<Settings>(), resolver.GetService<IModProvider>(), resolver.GetService<IModInstaller>(), resolver.GetService<IModVersionComparer>(), resolver.GetService<IGameVersionProvider>()));
            services.RegisterLazySingleton(() => new OptionsViewModel(resolver.GetService<ModsViewModel>(), resolver.GetService<Settings>(), resolver.GetService<IInstallDirValidator>()));
            services.RegisterLazySingleton(() => new MainWindowViewModel(resolver.GetService<ModsViewModel>()));
        }
    }
}