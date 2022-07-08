using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// A control used to indicate the progress of an operation.
    /// </summary>
    public class ProgressRing : RangeBase
    {
        /// <inheritdoc cref="ProgressBar.IsIndeterminateProperty"/>
        public static readonly StyledProperty<bool> IsIndeterminateProperty = ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>();

        /// <inheritdoc cref="Shape.StrokeThicknessProperty"/>
        public static readonly StyledProperty<double> StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner<ProgressRing>();

        /// <inheritdoc cref="Arc.StartAngleProperty"/>
        public static readonly StyledProperty<double> StartAngleProperty = Arc.StartAngleProperty.AddOwner<ProgressRing>();

        /// <inheritdoc cref="Arc.SweepAngleProperty"/>
        public static readonly StyledProperty<double> SweepAngleProperty = Arc.SweepAngleProperty.AddOwner<ProgressRing>();

        static ProgressRing()
        {
            MaximumProperty.Changed.Subscribe(CalibrateAngles);
            MinimumProperty.Changed.Subscribe(CalibrateAngles);
            ValueProperty.Changed.Subscribe(CalibrateAngles);
            StrokeThicknessProperty.OverrideDefaultValue<ProgressRing>(20);
            AffectsRender<ProgressRing>(StartAngleProperty, SweepAngleProperty);
        }

        /// <inheritdoc cref="ProgressBar.IsIndeterminate"/>
        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <inheritdoc cref="Shape.StrokeThickness"/>
        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <inheritdoc cref="Arc.StartAngle"/>
        public double StartAngle
        {
            get => GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        /// <inheritdoc cref="Arc.SweepAngle"/>
        public double SweepAngle
        {
            get => GetValue(SweepAngleProperty);
            private set => SetValue(SweepAngleProperty, value);
        }

        private static void CalibrateAngles(AvaloniaPropertyChangedEventArgs<double> e)
        {
            if (e.Sender is not ProgressRing pr) return;
            pr.SweepAngle = (pr.Value - pr.Minimum) / (pr.Maximum - pr.Minimum) * 360;
        }
    }
}
