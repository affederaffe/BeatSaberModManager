using System;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// A <see cref="DataGrid"/> which is searchable through a <see cref="TextBox"/>
    /// </summary>
    public class SearchableDataGrid : DataGrid
    {
        /// <inheritdoc cref="TextBox.WatermarkProperty" />
        public static readonly StyledProperty<string?> WatermarkProperty = TextBox.WatermarkProperty.AddOwner<SearchableDataGrid>();

        /// <summary>
        /// Defines the QueryProperty.
        /// </summary>
        public static readonly StyledProperty<string?> QueryProperty = AvaloniaProperty.Register<SearchableDataGrid, string?>(nameof(Query), defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Defines the IsSearchEnabledProperty.
        /// </summary>
        public static readonly StyledProperty<bool> IsSearchEnabledProperty = AvaloniaProperty.Register<SearchableDataGrid, bool>(nameof(IsSearchEnabled), defaultBindingMode: BindingMode.TwoWay);

        private TextBox? _searchTextBox;

        /// <inheritdoc cref="TextBox.Text" />
        public string? Query
        {
            get => GetValue(QueryProperty);
            set => SetValue(QueryProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the search text box is enabled for user interaction.
        /// </summary>
        public bool IsSearchEnabled
        {
            get => GetValue(IsSearchEnabledProperty);
            set => SetValue(IsSearchEnabledProperty, value);
        }

        /// <inheritdoc cref="TextBox.Watermark" />
        public string? Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            base.OnApplyTemplate(e);
            _searchTextBox = e.NameScope.Get<TextBox>("PART_SearchTextBox");
            CaptionButtons? captionButtons = ChromeOverlayLayer.GetOverlayLayer(this)?.FindDescendantOfType<CaptionButtons>();
            if (captionButtons is not null)
                e.NameScope.Get<ToggleButton>("PART_SearchToggleButton").Margin = new Thickness(0, 0, captionButtons.Bounds.Width, 0);
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            ArgumentNullException.ThrowIfNull(change);
            base.OnPropertyChanged(change);
            if (change.Property == IsSearchEnabledProperty && change.GetNewValue<bool>())
                _searchTextBox?.Focus(NavigationMethod.Pointer);
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            if (ItemsSource is DataGridCollectionView dataGridCollectionView)
                dataGridCollectionView.MoveCurrentTo(null);
        }
    }
}
