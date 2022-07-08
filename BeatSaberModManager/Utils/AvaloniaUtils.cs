using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

using Avalonia.Markup.Xaml;
using Avalonia.Styling;


namespace BeatSaberModManager.Utils
{
    /// <summary>
    /// Utilities for Avalonia-related operations.
    /// </summary>
    public static class AvaloniaUtils
    {
        /// <summary>
        /// Attempts to parse a XAML <see cref="IStyle"/> at runtime.
        /// </summary>
        /// <param name="xaml">The string containing the XAML.</param>
        /// <param name="dir">The directory of the file.</param>
        /// <param name="style">The parsed <see cref="IStyle"/> if the operation succeeds, null otherwise.</param>
        /// <typeparam name="T">The type of the <see cref="IStyle"/>.</typeparam>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public static bool TryParse<T>(string xaml, string dir, [MaybeNullWhen(false)] out T style) where T : class, IStyle
        {
            try
            {
                style = (T)AvaloniaRuntimeXamlLoader.Load(xaml, null, null, new Uri(dir));
                return true;
            }
            catch (ArgumentException) { }
            catch (XmlException) { }
            style = null;
            return false;
        }
    }
}
