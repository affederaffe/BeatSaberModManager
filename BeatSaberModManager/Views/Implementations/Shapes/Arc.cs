using System;

using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;


namespace BeatSaberModManager.Views.Implementations.Shapes
{
    public class Arc : Shape
    {
        private readonly ArcSegment _arcSegment;
        private readonly PathFigure _arcFigure;
        private readonly PathGeometry _arcGeometry;

        private readonly PathGeometry _emptyGeometry = new()
        {
            Figures =
            {
                new PathFigure
                {
                    StartPoint = new Point(0, 0)
                }
            }
        };

        public static readonly StyledProperty<double> StartAngleProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(StartAngle));

        public static readonly StyledProperty<double> EndAngleProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(EndAngle));

        static Arc()
        {
            StrokeThicknessProperty.OverrideDefaultValue<Arc>(1);
            AffectsGeometry<Arc>(BoundsProperty, StrokeThicknessProperty, StartAngleProperty, EndAngleProperty);
        }

        public Arc()
        {
            _arcSegment = new ArcSegment { SweepDirection = SweepDirection.Clockwise };

            _arcFigure = new PathFigure { IsClosed = false };
            _arcFigure.Segments ??= new PathSegments();
            _arcFigure.Segments.Add(_arcSegment);

            _arcGeometry = new PathGeometry();
            _arcGeometry.Figures.Add(_arcFigure);
        }

        public double StartAngle
        {
            get => GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public double EndAngle
        {
            get => GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        protected override Geometry CreateDefiningGeometry()
        {
            double angle1 = DegreesToRad(StartAngle);
            double angle2 = DegreesToRad(EndAngle);

            double startAngle = Math.Min(angle1, angle2);
            double endAngle = Math.Max(angle1, angle2);

            double normStart = RadToNormRad(startAngle);
            double normEnd = RadToNormRad(endAngle);

            Rect rect = new(Bounds.Size);

            if (Math.Abs(normStart - normEnd) < 0.01 && Math.Abs(startAngle - endAngle) > 0.01) //complete ring
                return new EllipseGeometry(rect.Deflate(StrokeThickness / 2));
            if (Math.Abs(normStart - normEnd) < 0.01 && Math.Abs(startAngle - endAngle) < 0.01) //empty
                return _emptyGeometry;

            Rect deflatedRect = rect.Deflate(StrokeThickness / 2);

            double centerX = rect.Center.X;
            double centerY = rect.Center.Y;

            double radiusX = deflatedRect.Width / 2;
            double radiusY = deflatedRect.Height / 2;

            double angleGap = RadToNormRad(endAngle - startAngle);

            Point startPoint = GetRingPoint(radiusX, radiusY, centerX, centerY, startAngle);
            Point endPoint = GetRingPoint(radiusX, radiusY, centerX, centerY, endAngle);

            _arcFigure.StartPoint = startPoint;

            _arcSegment.Point = endPoint;
            _arcSegment.IsLargeArc = angleGap >= Math.Tau / 2;
            _arcSegment.Size = new Size(radiusX, radiusY);

            return _arcGeometry;
        }

        public override void Render(DrawingContext context)
        {
            double angle1 = DegreesToRad(StartAngle);
            double angle2 = DegreesToRad(EndAngle);

            double startAngle = Math.Min(angle1, angle2);
            double endAngle = Math.Max(angle1, angle2);

            double normStart = RadToNormRad(startAngle);
            double normEnd = RadToNormRad(endAngle);


            if (Math.Abs(normStart - normEnd) > 0.01 || Math.Abs(startAngle - endAngle) > 0.01)
                base.Render(context);
        }

        private static double DegreesToRad(double inAngle) =>
            inAngle * Math.PI / 180;

        private static double RadToNormRad(double inAngle) =>
            (0 + inAngle % Math.Tau + Math.Tau) % Math.Tau;


        private static Point GetRingPoint(double radiusX, double radiusY, double centerX, double centerY, double angle) =>
            new(radiusX * Math.Cos(angle) + centerX, radiusY * Math.Sin(angle) + centerY);
    }
}