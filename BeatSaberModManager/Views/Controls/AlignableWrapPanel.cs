using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;


namespace BeatSaberModManager.Views.Controls
{
    /// <summary>
    /// TODO
    /// </summary>
    public class AlignableWrapPanel : WrapPanel
    {
        /// <summary>
        /// Defines the <see cref="HorizontalContentAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty = ContentControl.HorizontalContentAlignmentProperty.AddOwner<AlignableWrapPanel>();

        /// <summary>
        /// Defines the <see cref="VerticalContentAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty = ContentControl.VerticalContentAlignmentProperty.AddOwner<AlignableWrapPanel>();

        /// <summary>
        /// Gets or sets the horizontal alignment of the content within the control.
        /// </summary>
        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the content within the control.
        /// </summary>
        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        /// <inheritdoc />
        protected override Size MeasureOverride(Size constraint)
        {
            UVSize curLineSize = new(Orientation);
            UVSize panelSize = new(Orientation);
            UVSize uvConstraint = new(Orientation, constraint.Width, constraint.Height);
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool itemWidthSet = !double.IsNaN(itemWidth);
            bool itemHeightSet = !double.IsNaN(itemHeight);

            Size childConstraint = new(
                    itemWidthSet ? itemWidth : constraint.Width,
                    itemHeightSet ? itemHeight : constraint.Height);

            Avalonia.Controls.Controls children = Children;

            for (int i = 0, count = children.Count; i < count; i++)
            {
                Control child = children[i];

                //Flow passes its own constraint to children
                child.Measure(childConstraint);

                //this is the size of the child in UV space
                UVSize sz = new(
                        Orientation,
                        itemWidthSet ? itemWidth : child.DesiredSize.Width,
                        itemHeightSet ? itemHeight : child.DesiredSize.Height);

                if (curLineSize.Width + sz.Width > uvConstraint.Width)
                {
                    //need to switch to another line
                    panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
                    panelSize.Height += curLineSize.Height;
                    curLineSize = sz;

                    if (!(sz.Width > uvConstraint.Width))
                        continue;

                    //the element is wider then the constraint - give it a separate line
                    panelSize.Width = Math.Max(sz.Width, panelSize.Width);
                    panelSize.Height += sz.Height;
                    curLineSize = new UVSize(Orientation);
                }
                else
                {
                    //continue to accumulate a line
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            //the last line size, if any should be added
            panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
            panelSize.Height += curLineSize.Height;

            //go from UV space to W/H space
            return new Size(panelSize.Width, panelSize.Height);
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            int firstInLine = 0;
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            double accumulatedV = 0;
            double itemU = Orientation == Orientation.Horizontal ? itemWidth : itemHeight;
            UVSize curLineSize = new(Orientation);
            UVSize uvFinalSize = new(Orientation, finalSize.Width, finalSize.Height);
            bool itemWidthSet = !double.IsNaN(itemWidth);
            bool itemHeightSet = !double.IsNaN(itemHeight);
            bool useItemU = Orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;

            Avalonia.Controls.Controls children = Children;

            for (int i = 0, count = children.Count; i < count; i++)
            {
                Control child = children[i];

                UVSize sz = new(
                        Orientation,
                        itemWidthSet ? itemWidth : child.DesiredSize.Width,
                        itemHeightSet ? itemHeight : child.DesiredSize.Height);

                if (curLineSize.Width + sz.Width > uvFinalSize.Width)
                {
                    //need to switch to another line
                    ArrangeLine(finalSize, accumulatedV, curLineSize, firstInLine, i, useItemU, itemU);

                    accumulatedV += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > uvFinalSize.Width)
                    {
                        //the element is wider then the constraint - give it a separate line
                        //switch to next line which only contain one element
                        ArrangeLine(finalSize, accumulatedV, sz, i, ++i, useItemU, itemU);

                        accumulatedV += sz.Height;
                        curLineSize = new UVSize(Orientation);
                    }

                    firstInLine = i;
                }
                else
                {
                    //continue to accumulate a line
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            //arrange the last line, if any
            if (firstInLine < children.Count)
                ArrangeLine(finalSize, accumulatedV, curLineSize, firstInLine, children.Count, useItemU, itemU);

            return finalSize;
        }

        private void ArrangeLine(Size finalSize, double v, UVSize line, int start, int end, bool useItemU, double itemU)
        {
            bool isHorizontal = Orientation == Orientation.Horizontal;
            double u = isHorizontal
                ? HorizontalContentAlignment switch
                {
                    HorizontalAlignment.Left => 0,
                    HorizontalAlignment.Center => (finalSize.Width - line.Width) / 2,
                    HorizontalAlignment.Right => finalSize.Width - line.Width,
                    _ => 0
                }
                : VerticalContentAlignment switch
                {
                    VerticalAlignment.Top => 0,
                    VerticalAlignment.Center => (finalSize.Height - line.Width) / 2,
                    VerticalAlignment.Bottom => finalSize.Height - line.Width,
                    _ => 0
                };

            Avalonia.Controls.Controls children = Children;
            for (int i = start; i < end; i++)
            {
                Control child = children[i];
                UVSize childSize = new(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                double layoutSlotU = useItemU ? itemU : childSize.Width;
                child.Arrange(new Rect(
                        isHorizontal ? u : v,
                        isHorizontal ? v : u,
                        isHorizontal ? layoutSlotU : line.Height,
                        isHorizontal ? line.Height : layoutSlotU));
                u += layoutSlotU;
            }
        }

        private struct UVSize
        {
            private readonly Orientation _orientation;
            private double _u;
            private double _v;

            internal UVSize(Orientation orientation, double width, double height)
            {
                _u = _v = 0d;
                _orientation = orientation;
                Width = width;
                Height = height;
            }

            internal UVSize(Orientation orientation)
            {
                _u = _v = 0d;
                _orientation = orientation;
            }

            internal double Width
            {
                get => _orientation == Orientation.Horizontal ? _u : _v;
                set
                {
                    if (_orientation == Orientation.Horizontal)
                        _u = value;
                    else
                        _v = value;
                }
            }
            internal double Height
            {
                get => _orientation == Orientation.Horizontal ? _v : _u;
                set
                {
                    if (_orientation == Orientation.Horizontal)
                        _v = value;
                    else
                        _u = value;
                }
            }
        }
    }
}
