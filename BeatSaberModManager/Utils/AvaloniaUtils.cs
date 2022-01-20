using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

using Avalonia.Markup.Xaml;


namespace BeatSaberModManager.Utils
{
    public static class AvaloniaUtils
    {
        public static bool TryParse<T>(string xaml, string dir, [MaybeNullWhen(false)] out T style)
        {
            style = default;
            try
            {
                style = (T)AvaloniaRuntimeXamlLoader.Load(xaml, null, null, new Uri(dir));
                return true;
            }
            catch (ArgumentException) { }
            catch (XmlException) { }
            return false;
        }
    }
}