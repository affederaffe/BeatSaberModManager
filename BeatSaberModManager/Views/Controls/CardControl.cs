using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// TODO
    /// </summary>
    public class CardControl : ContentControl
    {
        /// <summary>
        /// 
        /// </summary>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<CardControl, object>(nameof(Header));

        /// <summary>
        /// 
        /// </summary>
        public ITemplate HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<ITemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<CardControl, ITemplate>(nameof(HeaderTemplate));

        /// <summary>
        /// 
        /// </summary>
        public object SecondaryHeader
        {
            get => GetValue(SecondaryHeaderProperty);
            set => SetValue(SecondaryHeaderProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<object> SecondaryHeaderProperty =
            AvaloniaProperty.Register<CardControl, object>(nameof(SecondaryHeader));

        /// <summary>
        /// 
        /// </summary>
        public ITemplate SecondaryHeaderTemplate
        {
            get => GetValue(SecondaryHeaderTemplateProperty);
            set => SetValue(SecondaryHeaderTemplateProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<ITemplate> SecondaryHeaderTemplateProperty =
            AvaloniaProperty.Register<CardControl, ITemplate>(nameof(SecondaryHeaderTemplate));

        /// <summary>
        /// 
        /// </summary>
        public IBrush SecondaryBackground
        {
            get => GetValue(SecondaryBackgroundProperty);
            set => SetValue(SecondaryBackgroundProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<IBrush> SecondaryBackgroundProperty =
            AvaloniaProperty.Register<CardControl, IBrush>(nameof(SecondaryBackground));

        /// <summary>
        /// 
        /// </summary>
        public bool ScaleOnPointerOver
        {
            get => GetValue(ScaleOnPointerOverProperty);
            set => SetValue(ScaleOnPointerOverProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<bool> ScaleOnPointerOverProperty =
            AvaloniaProperty.Register<CardControl, bool>(nameof(ScaleOnPointerOver));

        /// <summary>
        /// 
        /// </summary>
        public BoxShadows BoxShadow
        {
            get => GetValue(BoxShadowProperty);
            set => SetValue(BoxShadowProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
            AvaloniaProperty.Register<CardControl, BoxShadows>(nameof(BoxShadow));

        /// <summary>
        /// 
        /// </summary>
        public BoxShadows InternalBoxShadow
        {
            get => GetValue(InternalBoxShadowProperty);
            set => SetValue(InternalBoxShadowProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<BoxShadows> InternalBoxShadowProperty =
            AvaloniaProperty.Register<CardControl, BoxShadows>(nameof(InternalBoxShadow));

        /// <summary>
        /// Defines the Top CornerRadius
        /// </summary>
        public CornerRadius TopCornerRadius
        {
            get => GetValue(TopCornerRadiusProperty);
            set => SetValue(TopCornerRadiusProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<CornerRadius> TopCornerRadiusProperty =
            AvaloniaProperty.Register<CardControl, CornerRadius>(nameof(TopCornerRadius), new CornerRadius(7, 0));

        /// <summary>
        /// Defines the Bottom CornerRadius
        /// </summary>
        public CornerRadius BottomCornerRadius
        {
            get => GetValue(BottomCornerRadiusProperty);
            set => SetValue(BottomCornerRadiusProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<CornerRadius> BottomCornerRadiusProperty =
            AvaloniaProperty.Register<CardControl, CornerRadius>(nameof(BottomCornerRadius), new CornerRadius(0, 7));

        /// <summary>
        /// 
        /// </summary>
        public CornerRadius InternalCornerRadius
        {
            get => GetValue(InternalCornerRadiusProperty);
            set => SetValue(InternalCornerRadiusProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<CornerRadius> InternalCornerRadiusProperty =
            AvaloniaProperty.Register<CardControl, CornerRadius>(nameof(InternalCornerRadius), new CornerRadius(7));

        /// <summary>
        /// 
        /// </summary>
        public Thickness InternalPadding
        {
            get => GetValue(InternalPaddingProperty);
            set => SetValue(InternalPaddingProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StyledProperty<Thickness> InternalPaddingProperty =
            AvaloniaProperty.Register<CardControl, Thickness>(nameof(InternalPadding));
    }
}
