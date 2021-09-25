using System;

using Avalonia;
using Avalonia.Controls.Primitives;


namespace BeatSaberModManager.Views.Implementations.Controls
{
    public class ProgressRing : RangeBase
    {
        public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate));
        public static readonly StyledProperty<int> StrokeThicknessProperty = AvaloniaProperty.Register<ProgressRing, int>(nameof(StrokeThickness), 20);
        public static readonly DirectProperty<ProgressRing, double> StartAngleProperty = AvaloniaProperty.RegisterDirect<ProgressRing, double>(nameof(StartAngle), o => o.StartAngle);
        public static readonly DirectProperty<ProgressRing, double> SweepAngleProperty = AvaloniaProperty.RegisterDirect<ProgressRing, double>(nameof(SweepAngle), o => o.SweepAngle);

        static ProgressRing()
        {
            MaximumProperty.Changed.Subscribe(CalibrateAngles);
            MinimumProperty.Changed.Subscribe(CalibrateAngles);
            ValueProperty.Changed.Subscribe(CalibrateAngles);
            MaximumProperty.OverrideMetadata<ProgressRing>(new DirectPropertyMetadata<double>(100));
            MinimumProperty.OverrideMetadata<ProgressRing>(new DirectPropertyMetadata<double>());
            ValueProperty.OverrideMetadata<ProgressRing>(new DirectPropertyMetadata<double>(25));
            AffectsRender<ProgressRing>(StartAngleProperty, SweepAngleProperty);
        }

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public int StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        private double _startAngle;
        public double StartAngle
        {
            get => _startAngle;
            private set => SetAndRaise(StartAngleProperty, ref _startAngle, value);
        }

        private double _sweepAngle;
        public double SweepAngle
        {
            get => _sweepAngle;
            private set => SetAndRaise(SweepAngleProperty, ref _sweepAngle, value);
        }

        private static void CalibrateAngles(AvaloniaPropertyChangedEventArgs<double> e)
        {
            if (e.Sender is not ProgressRing pr) return;
            pr.StartAngle = -90;
            pr.SweepAngle = (pr.Value - pr.Minimum) / (pr.Maximum - pr.Minimum) * 360;
        }
    }
}