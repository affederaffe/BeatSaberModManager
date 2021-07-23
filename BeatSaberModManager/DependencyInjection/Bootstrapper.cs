using Splat;


namespace BeatSaberModManager.DependencyInjection
{
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            ServicesBootstrapper.RegisterServices(services, resolver);
            ViewModelBootstrapper.RegisterServices(services, resolver);
        }
    }
}