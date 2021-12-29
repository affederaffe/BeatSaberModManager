using System.Globalization;

using Avalonia.Controls;

using BeatSaberModManager.Views.Interfaces;


namespace BeatSaberModManager.Views.Implementations.Localization
{
    public class Language : ILanguage
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