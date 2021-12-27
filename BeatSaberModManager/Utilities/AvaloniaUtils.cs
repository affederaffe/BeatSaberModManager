using System.Diagnostics.CodeAnalysis;
using System.Xml;

using Avalonia.Markup.Xaml;


namespace BeatSaberModManager.Utilities
{
    public static class AvaloniaUtils
    {
        public static bool TryParse<T>(string xaml, [MaybeNullWhen(false)] out T style)
        {
            style = default;
            try
            {
                style = AvaloniaRuntimeXamlLoader.Parse<T>(xaml);
                return true;
            }
            catch (XmlException) { }
            return false;
        }
    }
}