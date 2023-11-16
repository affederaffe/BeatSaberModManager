using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// Advanced <see cref="TabControl"/> with taggable side bar
    /// </summary>
    public class HamburgerMenu : TabControl
    {
        private SplitView? _splitView;

        /// <summary>
        /// Defines the <see cref="PaneBackgroundProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> PaneBackgroundProperty = SplitView.PaneBackgroundProperty.AddOwner<HamburgerMenu>();

        /// <summary>
        /// Defines the <see cref="ContentBackgroundProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> ContentBackgroundProperty = AvaloniaProperty.Register<HamburgerMenu, IBrush?>(nameof(ContentBackground));

        /// <summary>
        /// Defines the <see cref="ExpandedModeThresholdWidthProperty "/> property.
        /// </summary>
        public static readonly StyledProperty<int> ExpandedModeThresholdWidthProperty = AvaloniaProperty.Register<HamburgerMenu, int>(nameof(ExpandedModeThresholdWidth), 1008);

        /// <summary>
        /// Defines the <see cref="ContentMarginProperty "/> property.
        /// </summary>
        public static readonly StyledProperty<Thickness> ContentMarginProperty = AvaloniaProperty.Register<HamburgerMenu, Thickness>(nameof(ContentMargin));

        /// <summary>
        /// Gets or sets the brush used to draw the pane's background.
        /// </summary>
        public IBrush? PaneBackground
        {
            get => GetValue(PaneBackgroundProperty);
            set => SetValue(PaneBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used to draw the content's background.
        /// </summary>
        public IBrush? ContentBackground
        {
            get => GetValue(ContentBackgroundProperty);
            set => SetValue(ContentBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the width necessary to toggle the expanded mode.
        /// </summary>
        public int ExpandedModeThresholdWidth
        {
            get => GetValue(ExpandedModeThresholdWidthProperty);
            set => SetValue(ExpandedModeThresholdWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets margin of the content.
        /// </summary>
        public Thickness ContentMargin
        {
            get => GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            base.OnApplyTemplate(e);
            _splitView = e.NameScope.Find<SplitView>("PART_NavigationPane");
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            ArgumentNullException.ThrowIfNull(change);
            base.OnPropertyChanged(change);
            if (change.Property != BoundsProperty || _splitView is null)
                return;
            (Rect oldBounds, Rect newBounds) = change.GetOldAndNewValue<Rect>();
            EnsureSplitViewMode(oldBounds, newBounds);
        }

        private void EnsureSplitViewMode(Rect oldBounds, Rect newBounds)
        {
            if (_splitView is null)
                return;
            int threshold = ExpandedModeThresholdWidth;
            if (newBounds.Width >= threshold && oldBounds.Width < threshold)
            {
                _splitView.DisplayMode = SplitViewDisplayMode.Inline;
                _splitView.IsPaneOpen = true;
            }
            else if (newBounds.Width < threshold && oldBounds.Width >= threshold)
            {
                _splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                _splitView.IsPaneOpen = false;
            }
        }
    }
}
