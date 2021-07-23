using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;

using BeatSaberModManager.Models;
using BeatSaberModManager.Models.Implementations.BeatSaber;
using BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods;
using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Utils;

using Splat;


namespace BeatSaberModManager.DependencyInjection
{
    public static class ServicesBootstrapper
    {
        public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton(Settings.Load);

            services.RegisterLazySingleton(() =>
            {
                HttpClientHandler clientHandler = new() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
                HttpClient client = new(clientHandler) { Timeout = TimeSpan.FromSeconds(30) };
                client.DefaultRequestHeaders.Add("User-Agent", $"BeatSaberModManager/{typeof(App).Assembly.GetName().Version}");
                return client;
            });

            services.RegisterLazySingleton(() => new JsonSerializerOptions
            {
                Converters =
                {
                    new ConcreteConverter<IMod, BeatModsMod>(),
                    new ConcreteConverter<IHash, BeatModsHash>(),
                    new ConcreteConverter<IAuthor, BeatModsAuthor>(),
                    new ConcreteConverter<IDownload, BeatModsDownload>(),
                    new ConcreteConverter<IDependency, BeatModsDependency>()
                }
            }
            );

            services.RegisterLazySingleton(() =>
                new MD5HashProvider(),
                typeof(IHashProvider)
            );

            services.RegisterLazySingleton(() =>
                new SemVerModVersionComparer(),
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
                new BeatModsModProvider(resolver.GetService<Settings>(), resolver.GetService<HttpClient>(), resolver.GetService<JsonSerializerOptions>(), resolver.GetService<IHashProvider>()),
                typeof(IModProvider)
            );

            services.RegisterLazySingleton(() =>
                new BeatModsModInstaller(resolver.GetService<Settings>(), resolver.GetService<IModProvider>(), resolver.GetService<IHashProvider>()),
                typeof(IModInstaller)
            );
        }
    }
}