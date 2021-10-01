using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;


namespace BeatSaberModManager.Views.Implementations.Controls
{
    public class ProgressRing : RangeBase
    {
        public static readonly StyledProperty<bool> IsIndeterminateProperty = ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>();
        public static readonly StyledProperty<double> StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner<ProgressRing>();
        public static readonly StyledProperty<double> StartAngleProperty = Arc.StartAngleProperty.AddOwner<ProgressRing>();
        public static readonly StyledProperty<double> SweepAngleProperty = Arc.SweepAngleProperty.AddOwner<ProgressRing>();

        static ProgressRing()
        {
            MaximumProperty.Changed.Subscribe(CalibrateAngles);
            MinimumProperty.Changed.Subscribe(CalibrateAngles);
            ValueProperty.Changed.Subscribe(CalibrateAngles);
            StrokeThicknessProperty.OverrideDefaultValue<ProgressRing>(20);
            AffectsRender<ProgressRing>(StartAngleProperty, SweepAngleProperty);
        }

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public double StartAngle
        {
            get => GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

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