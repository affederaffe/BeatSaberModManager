using System;
using System.Net;
using System.Net.Http;

using BeatSaberModManager.Localisation;
using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Implementations.BeatSaber;
using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Implementations.BeatSaber.BeatSaver;
using BeatSaberModManager.Models.Implementations.BeatSaber.ModelSaber;
using BeatSaberModManager.Models.Implementations.BeatSaber.Playlist;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Theming;

using Splat;


namespace BeatSaberModManager.DependencyInjection
{
    public static class ServicesBootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton(Settings.Load);

            services.RegisterLazySingleton(() => new LanguageSwitcher(resolver.GetService<Settings>()));

            services.RegisterLazySingleton(() => new ThemeSwitcher(resolver.GetService<Settings>()));

            services.RegisterLazySingleton(() =>
            {
                HttpClientHandler clientHandler = new() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
                HttpClient client = new(clientHandler) { Timeout = TimeSpan.FromSeconds(30) };
                client.DefaultRequestHeaders.Add("User-Agent", $"{nameof(BeatSaberModManager)}/{Environment.Version}");
                return client;
            });

            services.RegisterLazySingleton(() =>
                new MD5HashProvider(),
                typeof(IHashProvider)
            );

            services.RegisterLazySingleton(() =>
                new SystemVersionComparer(),
                typeof(IModVersionComparer)
            );

            services.RegisterLazySingleton(() =>
                new BeatSaberInstallDirValidator(),
                typeof(IInstallDirValidator)
            );

            services.RegisterLazySingleton(() =>
                new BeatSaberInstallDirLocator(),
                typeof(IInstallDirLocator)
            );

            services.RegisterLazySingleton(() =>
                new BeatSaberGameVersionProvider(resolver.GetService<Settings>()),
                typeof(IGameVersionProvider)
            );

            services.RegisterLazySingleton(() =>
                new BeatModsModProvider(resolver.GetService<Settings>(), resolver.GetService<HttpClient>(), resolver.GetService<IHashProvider>(), resolver.GetService<IGameVersionProvider>()),
                typeof(IModProvider)
            );

            services.RegisterLazySingleton(() =>
                new BeatModsModInstaller(resolver.GetService<Settings>(), resolver.GetService<IModProvider>(), resolver.GetService<IHashProvider>()),
                typeof(IModInstaller)
            );

            services.RegisterLazySingleton(() =>
                new ModelSaberModelInstaller(resolver.GetService<Settings>(), resolver.GetService<HttpClient>()));

            services.RegisterLazySingleton(() =>
                new BeatSaverMapInstaller(resolver.GetService<Settings>(), resolver.GetService<HttpClient>()));

            services.RegisterLazySingleton(() =>
                new PlaylistInstaller(resolver.GetService<HttpClient>(),resolver.GetService<BeatSaverMapInstaller>()));

            services.RegisterLazySingleton(() =>
                new ModelSaberAssetProvider(resolver.GetService<ModelSaberModelInstaller>()),
                typeof(IAssetProvider)
            );

            services.RegisterLazySingleton(() =>
                new BeatSaverAssetProvider(resolver.GetService<BeatSaverMapInstaller>()),
                typeof(IAssetProvider)
            );

            services.RegisterLazySingleton(() =>
                new PlaylistAssetProvider(resolver.GetService<PlaylistInstaller>()),
                typeof(IAssetProvider)
            );
        }
    }
}