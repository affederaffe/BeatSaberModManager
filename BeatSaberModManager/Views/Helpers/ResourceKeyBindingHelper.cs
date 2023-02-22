using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;


namespace BeatSaberModManager.Views.Helpers
{
    /// <summary>
    /// Helper for using a <see cref="DynamicResourceExtension"/> with a <see cref="CompiledBindingExtension"/> as the key.
    /// </summary>
    public class ResourceKeyBindingHelper
    {
        /// <summary>
        /// Defines the SourceResourceKeyProperty property.
        /// </summary>
        public static readonly AttachedProperty<object> SourceResourceKeyProperty = AvaloniaProperty.RegisterAttached<ResourceKeyBindingHelper, ContentControl, object>("SourceResourceKey");

        static ResourceKeyBindingHelper()
        {
            SourceResourceKeyProperty.Changed.AddClassHandler<ContentControl>(OnSourceResourceKeyChanged);
        }

        private static void OnSourceResourceKeyChanged(ContentControl element, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.NewValue is null) return;
            element[!ContentControl.ContentProperty] = new DynamicResourceExtension(args.NewValue);
        }

        /// <summary>
        /// Helper for setting SourceResourceKey property on a Control.
        /// </summary>
        /// <param name="element">Control to set SourceResourceKey property on.</param>
        /// <param name="value">SourceResourceKey property value.</param>
        public static void SetSourceResourceKey(ContentControl element, object value) => element.SetValue(SourceResourceKeyProperty, value);

        /// <summary>
        /// Helper for reading Column property from a Control.
        /// </summary>
        /// <param name="element">Control to read SourceResourceKey property from.</param>
        /// <returns>Column property value.</returns>
        public static object GetSourceResourceKey(ContentControl element) => element.GetValue(SourceResourceKeyProperty);
    }
}
