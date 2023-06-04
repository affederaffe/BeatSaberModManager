using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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

        /// <summary>
        /// Defines the SearchIconProperty.
        /// </summary>
        public static readonly StyledProperty<PathIcon> SearchIconProperty = AvaloniaProperty.Register<SearchableDataGrid, PathIcon>(nameof(SearchIcon));

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

        /// <summary>
        /// Gets or sets the icon of the search icon.
        /// </summary>
        public PathIcon SearchIcon
        {
            get => GetValue(SearchIconProperty);
            set => SetValue(SearchIconProperty, value);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _searchTextBox = e.NameScope.Get<TextBox>("PART_SearchTextBox");
            CaptionButtons? captionButtons = ChromeOverlayLayer.GetOverlayLayer(this)?.FindDescendantOfType<CaptionButtons>();
            if (captionButtons is null) return;
            e.NameScope.Get<ToggleButton>("PART_SearchToggleButton").Margin = new Thickness(0, 0, captionButtons.Bounds.Width, 0);
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsSearchEnabledProperty && change.GetNewValue<bool>())
                _searchTextBox?.Focus();
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            if (ItemsSource is DataGridCollectionView dataGridCollectionView)
                dataGridCollectionView.MoveCurrentTo(null);
        }
    }
}
