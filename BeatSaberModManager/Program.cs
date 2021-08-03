using Avalonia;
using Avalonia.ReactiveUI;

using BeatSaberModManager.Utils;


namespace BeatSaberModManager
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            switch (args.Length is 0 ? null : args[0])
            {
                case "--register" when args.Length > 3:
                    PlatformUtils.RegisterProtocolHandler(args[1], args[2], args[3]);
                    break;
                case "--unregister" when args.Length > 2:
                    PlatformUtils.UnregisterProtocolHandler(args[1], args[2]);
                    break;
                default:
                    AppBuilder.Configure<App>().UsePlatformDetect().UseReactiveUI().LogToTrace().StartWithClassicDesktopLifetime(args);
                    break;
            }
        }
    }
}