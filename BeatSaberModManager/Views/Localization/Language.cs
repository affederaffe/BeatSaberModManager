using System.Globalization;

using Avalonia.Controls;


namespace BeatSaberModManager.Views.Localization
{
    /// <summary>
    /// Represents a language used for localization.
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Language"/> class.
        /// </summary>
        public Language(CultureInfo cultureInfo, IResourceProvider resourceProvider)
        {
            CultureInfo = cultureInfo;
            ResourceProvider = resourceProvider;
        }

        /// <summary>
        /// The <see cref="CultureInfo"/> of the language.
        /// </summary>
        public CultureInfo CultureInfo { get; }

        /// <summary>
        /// The resources used for localization.
        /// </summary>
        public IResourceProvider ResourceProvider { get; }
    }
}
