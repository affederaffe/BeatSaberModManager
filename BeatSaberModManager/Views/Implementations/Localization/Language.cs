using System.Globalization;

using Avalonia.Controls;


namespace BeatSaberModManager.Views.Implementations.Localization
{
    public class Language
    {
        public Language(CultureInfo cultureInfo, IResourceProvider resourceProvider)
        {
            CultureInfo = cultureInfo;
            ResourceProvider = resourceProvider;
        }

        public CultureInfo CultureInfo { get; }
        public IResourceProvider ResourceProvider { get; }
    }
}