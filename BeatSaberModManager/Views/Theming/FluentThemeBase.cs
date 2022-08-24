using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;


namespace BeatSaberModManager.Views.Theming
{
    /// <summary>
    /// Includes the fluent theme in an application.
    /// </summary>
    public class FluentThemeBase : AvaloniaObject, IStyle, IResourceProvider
    {
        private readonly Styles _styles;

        /// <summary>
        /// Defines the <see cref="Style"/> property.
        /// </summary>
        public static readonly StyledProperty<IStyle?> StyleProperty = AvaloniaProperty.Register<FluentThemeBase, IStyle?>(nameof(Style));

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentThemeBase"/> class.
        /// </summary>
        /// <param name="baseUri">The base URL for the XAML context.</param>
        public FluentThemeBase(Uri baseUri)
        {
            Styles sharedStyles = new()
            {
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/AccentColors.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/Base.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Controls/FluentControls.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://BeatSaberModManager/Resources/Styles/Brushes.axaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://BeatSaberModManager/Resources/Styles/Controls.axaml") }
            };

            FluentLight = new Styles
            {
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/BaseLight.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentControlResourcesLight.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml") }
            };

            FluentDark = new Styles
            {
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/BaseDark.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentControlResourcesDark.xaml") },
                new StyleInclude(baseUri) { Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml") }
            };

            _styles = new Styles { sharedStyles, FluentDark };
        }

        /// <summary>
        /// Gets or sets the additional style of the theme.
        /// </summary>
        public IStyle? Style
        {
            get => GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        /// <inheritdoc />
        public bool HasResources => (_styles as IResourceProvider).HasResources;

        /// <inheritdoc />
        public IResourceHost? Owner => (_styles as IResourceProvider).Owner;

        /// <inheritdoc />
        public SelectorMatchResult TryAttach(IStyleable target, object? host) => _styles.TryAttach(target, host);

        /// <inheritdoc />
        public IReadOnlyList<IStyle> Children => (_styles as IStyle).Children;

        /// <summary>
        /// Gets the basic fluent dark theme.
        /// </summary>
        public Styles FluentDark { get; }

        /// <summary>
        /// Gets the basic fluent light theme.
        /// </summary>
        public Styles FluentLight { get; }

        /// <inheritdoc />
        public event EventHandler? OwnerChanged
        {
            add => (_styles as IResourceProvider).OwnerChanged += value;
            remove => (_styles as IResourceProvider).OwnerChanged -= value;
        }

        /// <inheritdoc />
        public void AddOwner(IResourceHost owner) => (_styles as IResourceProvider).AddOwner(owner);

        /// <inheritdoc />
        public void RemoveOwner(IResourceHost owner) => (_styles as IResourceProvider).RemoveOwner(owner);

        /// <inheritdoc />
        public bool TryGetResource(object key, out object? value) => (_styles as IResourceProvider).TryGetResource(key, out value);

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property != StyleProperty) return;
            _styles[1] = change.GetNewValue<IStyle>();
        }
    }
}
